using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.RateLimiting;
using WebServiceCore.Models;
using WebServiceCore.Models.Entities;
using WebServiceCore.Services;

namespace WebServiceCore
{

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
            builder.WebHost.UseKestrel(opt =>
            {
                opt.Limits.MaxRequestBodySize = InputRules.MaxBodyBytes;
                opt.Limits.MaxConcurrentConnections = 100;
                opt.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);
            });
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<OnlineVideosDataContext>(opt => opt.UseSqlite(
                builder.Configuration.GetConnectionString("OnlineVideos") ?? "Data Source=Onlinevideos.db3"));
            builder.Services.AddMemoryCache(options => options.SizeLimit = 20000);
            builder.Services.AddScoped<Credentials>();
            builder.Services.AddScoped<PackageReview>();
            builder.Services.AddScoped<AccountManager>();
            builder.Services.AddScoped<SiteReviews>();
            builder.Services.AddScoped<Notifications>();
            builder.Services.AddHostedService<MailWorker>();

            var privateRoot = Path.GetFullPath(builder.Configuration["Security:PrivateDataPath"] ?? Path.Combine(builder.Environment.ContentRootPath, "private-data"));
            var publicRoot = Path.GetFullPath(builder.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot")).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if ((privateRoot + Path.DirectorySeparatorChar).StartsWith(publicRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                throw new InvalidOperationException("Private data must be outside wwwroot.");
            builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(privateRoot, "keys")));
            builder.Services.AddOnlineVideosServices();
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                // Defaults trust loopback proxies only. Never clear KnownProxies/KnownIPNetworks.
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.ForwardLimit = 1;
            });
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.AccessDeniedPath = "/Login";
                options.Cookie.Path = "/";
                options.Cookie.Name = "OnlineVideos.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(2);
                options.SlidingExpiration = false;
                options.Events.OnValidatePrincipal = async context =>
                {
                    var db = context.HttpContext.RequestServices.GetRequiredService<OnlineVideosDataContext>();
                    var user = await db.Users.FindAsync(context.Principal?.Identity?.Name);
                    if (user == null || user.IsDisabled || context.Principal?.FindFirstValue("credential-stamp") != Credentials.Stamp(user)
                        || context.Principal.IsInRole("admin") != user.IsAdmin)
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync();
                    }
                };
            });
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;
                options.OnRejected = (context, token) =>
                {
                    context.HttpContext.Response.Headers.RetryAfter = "60";
                    return ValueTask.CompletedTask;
                };
                options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                    PartitionedRateLimiter.Create<HttpContext, string>(_ => RateLimitPartition.GetConcurrencyLimiter("global", _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = 16,
                        QueueLimit = 0
                    })),
                    PartitionedRateLimiter.Create<HttpContext, string>(context =>
                        context.Request.Path.StartsWithSegments("/api/OnlineVideos/SubmitDll", StringComparison.OrdinalIgnoreCase) || context.Request.Path.StartsWithSegments("/api/OnlineVideos/SubmitSite", StringComparison.OrdinalIgnoreCase)
                        ? RateLimitPartition.GetConcurrencyLimiter("large-uploads", _ => new ConcurrencyLimiterOptions
                        {
                            PermitLimit = 2,
                            QueueLimit = 0
                        })
                        : RateLimitPartition.GetNoLimiter("other-requests")),
                    PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    {
                        var path = context.Request.Path.Value ?? "";
                        var login = path.Equals("/Login", StringComparison.OrdinalIgnoreCase) && HttpMethods.IsPost(context.Request.Method);

                        var write = !HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method);
                        var budget = login ? "login" : write ? "write" : "read";
                        var key = (context.Connection.RemoteIpAddress?.ToString() ?? "unknown") + ":" + budget;

                        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = login ? 5 : write ? 10 : 120,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        });
                    }));
            });
            var app = builder.Build();
            // Offline maintenance operations: no HTTP listener, no password in command-line arguments.
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();
                if (args.Length == 1 && args[0] == "--initialize-empty")
                {
                    await db.Database.EnsureCreatedAsync();
                    Console.WriteLine("Database schema initialized. No demo accounts were created.");
                    return;
                }
                await AccountSchema.Upgrade(db);
                await Credentials.Migrate(db);
                if (args.Length > 0 && args[0] is "--set-password" or "--create-user")
                {
                    if (args.Length != 2 || string.IsNullOrWhiteSpace(args[1]) || args[1].Length > 254)
                        throw new ValidationException("Supply exactly one user identifier; set OV_NEW_PASSWORD via environment.");

                    var password = Environment.GetEnvironmentVariable("OV_NEW_PASSWORD");
                    if (password == null || password.Length < 14 || password.Length > 256)
                        throw new ValidationException("Use a password of 14 to 256 characters.");

                    var user = await db.Users.FindAsync(args[1]);
                    if (args[0] == "--create-user")
                    {
                        if (user != null)
                            throw new ValidationException("Account already exists.");
                        user = new User
                        {
                            Email = args[1],
                            IsAdmin = Environment.GetEnvironmentVariable("OV_CREATE_ADMIN") == "true"
                        };
                        db.Users.Add(user);
                    }

                    if (user == null)
                        throw new ValidationException("Unknown user. Provision accounts offline through the operator.");
                    user.Password = Credentials.Hash(user, password);

                    await db.SaveChangesAsync();
                    Console.WriteLine("Password replaced; previous sessions are invalidated.");

                    return;
                }

                if (args.Length > 0 && args[0] == "--approve-package")
                {
                    if (args.Length != 2) throw new ValidationException("Usage: --approve-package REVIEW_ID");
                    await scope.ServiceProvider.GetRequiredService<PackageReview>().Approve(args[1]);
                    Console.WriteLine("Reviewed package published.");
                    return;
                }
            }
            app.UseForwardedHeaders();
            app.UseRateLimiter();
            app.Use(async (context, next) =>
            {
                context.Response.Headers.XContentTypeOptions = "nosniff";
                context.Response.Headers.XFrameOptions = "DENY";
                context.Response.Headers["Referrer-Policy"] = "same-origin";
                context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";

                var path = context.Request.Path;
                if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method)
                    && !path.StartsWithSegments("/api/OnlineVideos/SubmitDll", StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWithSegments("/api/OnlineVideos/SubmitSite", StringComparison.OrdinalIgnoreCase))
                {
                    const long formLimit = 32 * 1024;
                    if (context.Request.ContentLength > formLimit)
                    {
                        context.Response.StatusCode = 413;
                        return;
                    }
                    var bodyLimit = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
                    if (bodyLimit is { IsReadOnly: false })
                        bodyLimit.MaxRequestBodySize = formLimit;
                }

                if (path.StartsWithSegments("/Account") ||
                    path.StartsWithSegments("/Users") ||
                    path.StartsWithSegments("/Reviews") ||
                    path.StartsWithSegments("/Activate"))
                {
                    context.Response.Headers.CacheControl = "no-store";
                    context.Response.Headers["Referrer-Policy"] = "no-referrer";
                }

                // Allow plaintext catalogue reads for legacy clients, never credentials or mutations by default.
                if (!context.Request.IsHttps
                    && ((!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method)) || path.StartsWithSegments("/Login")
                        || path.StartsWithSegments("/Account") || path.StartsWithSegments("/Users") || path.StartsWithSegments("/Activate") || path.StartsWithSegments("/Reviews")
                        || path.StartsWithSegments("/api/OnlineVideos/GetDll", StringComparison.OrdinalIgnoreCase)
                        || path.StartsWithSegments("/api/OnlineVideos/GetSiteXml", StringComparison.OrdinalIgnoreCase)))
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("HTTPS is required for authentication and writes.");
                    return;
                }

                if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
                {
                    var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(app.Environment.ContentRootPath))!);
                    if (drive.AvailableFreeSpace < 512L * 1024 * 1024)
                    {
                        context.Response.StatusCode = 507;
                        await context.Response.WriteAsync("Storage reserve reached.");
                        return;
                    }
                }

                // Prevent bypass of API validation/limits through direct static package downloads or temp/backup files.
                if (path.StartsWithSegments("/Dlls", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                try
                {
                    await next(context);
                }
                catch (ValidationException) when (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid request.");
                }
                catch (IOException) when (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 503;
                    await context.Response.WriteAsync("Storage unavailable or busy.");
                }
            });

            app.UseExceptionHandler(handler => handler.Run(async context =>
            {
                var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
                context.Response.StatusCode = error is BadHttpRequestException bad ? bad.StatusCode : error is ValidationException ? 400 : error is IOException ? 503 : 500;

                await context.Response.WriteAsync(context.Response.StatusCode == 400 ? "Invalid request." : "Request could not be completed.");
            }));
            if (!app.Environment.IsDevelopment())
                app.UseHsts();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapRazorPages();

            await app.RunAsync();
        }
    }
}

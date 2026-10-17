using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebServiceCore.Models;
using WebServiceCore.Models.Entities;
using WebServiceCore.Services;

var results = new List<string>();
void Check(bool pass, string label) { if (!pass) throw new Exception(label); results.Add(label); Console.WriteLine("PASS: " + label); }
var root = Path.Combine(Path.GetTempPath(), "ov-security-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
var dbpath = Path.Combine(root, "test.db3");
var options = new DbContextOptionsBuilder<OnlineVideosDataContext>().UseSqlite("Data Source=" + dbpath).Options;
const string password = "Synthetic-Test-Password-2026!";
try
{
    using (var db = new OnlineVideosDataContext(options))
    {
        db.Database.EnsureCreated();
        var owner = new User { Email = "owner", Password = password, IsAdmin = false, IsPublisher = true };
        db.Users.AddRange(owner, new User { Email = "other", Password = password }, new User { Email = "admin", Password = password, IsAdmin = true });
        db.Sites.Add(new Site { Name = "Fixture", Owner = owner, Description = "Test", XML = "<Site />", Language = "en", LastUpdated = DateTime.UtcNow });
        db.SaveChanges();
    }
    var api = "/api/OnlineVideos/";
    async Task<int> Report(HttpClient client, int type, string email = null, string pass = null, string message = "Audit fixture") =>
        (int)(await client.PostAsJsonAsync(api + "SubmitReport", new { SiteName = "Fixture", Type = type, Message = message, Email = email, Password = pass })).StatusCode;
    async Task<int> Upload(HttpClient client, string email, byte[] bytes, string name = "Microsoft.Data.Sqlite") =>
        (int)(await client.PostAsJsonAsync(api + "SubmitDll", new { Email = email, Password = password, Name = name, Data = bytes })).StatusCode;
    using (var factory = new TestFactory(root, dbpath))
    using (var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false }))
    {
        using (var db = new OnlineVideosDataContext(options)) Check(db.Users.All(u => u.Password.StartsWith("OVH1:") && u.Password != password), "Legacy passwords migrated to salted hashes");
        Check(await Report(client, 2) == 401, "Anonymous confirmed-broken blocked");
        Check(await Report(client, 4, "owner", "wrong") == 401, "Wrong-password moderation blocked");
        Check(await Report(client, 2, "other", password) == 403, "Unrelated user moderation blocked");
        Check(await Report(client, 2, "owner", password) == 200, "Authenticated owner moderation accepted");
        Check(await Report(client, 1) == 200, "Legacy anonymous broken report remains accepted");
        using (var db = new OnlineVideosDataContext(options)) Check(db.Sites.Single().State == SiteState.Broken, "Anonymous report cannot change authoritative provider state");
        Check(await Report(client, 255) == 400, "Undefined report enum rejected");
        Check(await Report(client, 0, message: new string('x', 2001)) == 400, "Oversized report rejected");
        Check(await Upload(client, "admin", null) == 400, "Null DLL data returns validation error");
        Check(await Upload(client, "other", [1, 2, 3]) == 403, "Normal account cannot publish packages");
        Check(await Upload(client, "admin", [1, 2, 3]) == 400, "Approved publisher cannot stage a fake DLL");
        var throttled = await client.PostAsJsonAsync(api + "SubmitReport", new { SiteName = "Fixture", Type = 0, Message = "quota probe" });
        Check((int)throttled.StatusCode == 429 && throttled.Headers.Contains("Retry-After"), "Write rate limit returns 429 and Retry-After");
        Check((int)(await client.GetAsync(api + "RegisterEmail?email=fixture%40example.invalid")).StatusCode == 410, "Legacy password-email flow disabled");
        Check((int)(await client.GetAsync(api + "GetDll?name=../outside")).StatusCode == 400, "Traversal returns controlled validation error");
        Check((int)(await client.GetAsync("/Dlls/Microsoft.Data.Sqlite.dll")).StatusCode == 404, "Direct static DLL bypass blocked");
        File.WriteAllBytes(Path.Combine(root, "wwwroot", "Dlls", "Uncatalogued.dll"), [1, 2, 3]);
        Check((int)(await client.GetAsync(api + "GetDll?name=Uncatalogued")).StatusCode == 404, "Files without approved catalogue metadata are not downloadable");
    }
    using (var factory = new TestFactory(root, dbpath))
    using (var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false }))
    {
        var binary = File.ReadAllBytes(typeof(Microsoft.Data.Sqlite.SqliteConnection).Assembly.Location);
        var response = await client.PostAsJsonAsync(api + "SubmitDll", new { Email = "admin", Password = password, Name = "Microsoft.Data.Sqlite", Data = binary });
        Check((int)response.StatusCode == 202, "Managed assembly staged for review, not auto-published");
        var id = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("reviewId").GetString();
        Check((int)(await client.GetAsync(api + "GetDll?name=Microsoft.Data.Sqlite")).StatusCode == 404, "Unapproved package is not downloadable");
        using (var scope = factory.Services.CreateScope()) await scope.ServiceProvider.GetRequiredService<PackageReview>().Approve(id);
        var downloaded = await client.GetFromJsonAsync<byte[]>(api + "GetDll?name=Microsoft.Data.Sqlite");
        Check(downloaded.SequenceEqual(binary), "Explicit local approval publishes original package bytes");
        var dtd = "<!DOCTYPE Site [<!ENTITY x SYSTEM 'file:///nonexistent-security-fixture'>]><Site name='Fixture' lang='en'><Description>&x;</Description></Site>";
        var rejected = await client.PostAsJsonAsync(api + "SubmitSite", new { Email = "owner", Password = password, SiteXml = dtd });
        Check((int)rejected.StatusCode == 400, "DTD and external-entity XML rejected");
        var encoded = await Report(client, 0, message: "<script>void(0)</script>");
        var html = await client.GetStringAsync("/Reports/SiteReports/Fixture");
        Check(encoded == 200 && html.Contains("&lt;script&gt;") && !html.Contains("<script>void(0)</script>"), "Stored report markup is HTML encoded");
        // Staged metadata and package bytes must agree at approval time.
        response = await client.PostAsJsonAsync(api + "SubmitDll", new { Email = "admin", Password = password, Name = "Microsoft.Data.Sqlite", Data = binary });
        id = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("reviewId").GetString();
        File.AppendAllText(Path.Combine(root, "private", id + ".dll"), "tampered");
        bool mismatchRejected = false;
        using (var scope = factory.Services.CreateScope())
            try { await scope.ServiceProvider.GetRequiredService<PackageReview>().Approve(id); } catch (System.ComponentModel.DataAnnotations.ValidationException) { mismatchRejected = true; }
        Check(mismatchRejected, "Changed staged bytes rejected at approval");
    }
    using (var factory = new TestFactory(root, dbpath))
    using (var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false }))
    {
        var page = await client.GetStringAsync("/Login");
        var token = Regex.Match(page, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        var login = await client.PostAsync("/Login?handler=Login", new FormUrlEncodedContent(new Dictionary<string, string> { ["username"] = "admin", ["password"] = password, ["__RequestVerificationToken"] = token }));
        Check((int)login.StatusCode == 302, "Hashed-password browser login succeeds over HTTPS");
        var cookie = login.Headers.GetValues("Set-Cookie").First(v => v.StartsWith("OnlineVideos.Session="));
        Check(cookie.Contains("secure", StringComparison.OrdinalIgnoreCase) && cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase) && cookie.Contains("samesite=lax", StringComparison.OrdinalIgnoreCase), "Session cookie is Secure, HttpOnly and SameSite=Lax");
        Check((int)(await client.PostAsync("/Login?handler=Login", new FormUrlEncodedContent(new Dictionary<string, string> { ["username"] = "admin", ["password"] = password }))).StatusCode == 400, "Login antiforgery protection retained");
        using (var db = new OnlineVideosDataContext(options)) { var user = db.Users.Single(u => u.Email == "admin"); user.Password = Credentials.Hash(user, "Changed-Password-Test-Only"); db.SaveChanges(); }
        page = await client.GetStringAsync("/Login");
        Check(!page.Contains("Log out"), "Password replacement invalidates an existing session");
        using var http = factory.CreateClient(new() { BaseAddress = new Uri("http://localhost"), AllowAutoRedirect = false });
        Check((int)(await http.GetAsync("/Login")).StatusCode == 403, "Plain HTTP login blocked by default");
        Check(await Report(http, 0) == 403, "Plain HTTP mutations blocked by default");
        Check((int)(await http.GetAsync(api + "GetDll?name=Microsoft.Data.Sqlite")).StatusCode == 403, "Executable package download requires HTTPS");
        Check((int)(await http.GetAsync(api + "GetSiteXml?siteName=Fixture")).StatusCode == 403, "Provider configuration download requires HTTPS");
        Check((int)(await http.GetAsync(api + "GetSitesOverview")).StatusCode == 200, "Legacy catalogue read compatibility retained");
        using (var scope = factory.Services.CreateScope())
        {
            var credentials = scope.ServiceProvider.GetRequiredService<Credentials>();
            for (var i = 0; i < 5; i++) await credentials.Authenticate("other", "wrong");
            Check(await credentials.Authenticate("other", password) == null, "Account failure budget applies independently of client IP");
        }
    }
    using(var factory=new TestFactory(root,dbpath)) {
        using var browser=factory.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
        async Task Act(Func<AccountManager,Task> action){using var scope=factory.Services.CreateScope();await action(scope.ServiceProvider.GetRequiredService<AccountManager>());}
        async Task<bool> Denied(Func<AccountManager,Task> action){try{await Act(action);return false;}catch(Exception ex) when(ex is System.ComponentModel.DataAnnotations.ValidationException or UnauthorizedAccessException){return true;}}
        Check((int)(await browser.GetAsync("/Users")).StatusCode==302,"Anonymous account list requires login");
        Check(await Denied(a=>a.Edit("other","owner","admin",false)),"Ordinary users cannot grant administrator access");
        Check(await Denied(a=>a.Edit("admin","ADMIN","user",false)),"Case variant self-demotion cannot bypass checks");
        Check(await Denied(a=>a.Edit("admin","admin","user",false)),"Administrator cannot demote self");
        Check(await Denied(a=>a.Edit("admin","admin","admin",true)),"Administrator cannot disable self");
        await Act(a=>a.Edit("admin","other","publisher",false));
        using(var db=new OnlineVideosDataContext(options)) Check(db.Users.Single(u=>u.Email=="other").IsPublisher,"Administrator can grant publisher role");
        await Act(a=>a.Edit("admin","other","user",true));
        using(var scope=factory.Services.CreateScope()) Check(await scope.ServiceProvider.GetRequiredService<Credentials>().Authenticate("other",password)==null,"Disabled account cannot authenticate through API");
        await Act(a=>a.Edit("admin","other","user",false));
        Check(await Denied(a=>a.Delete("admin","admin","admin")),"Self-deletion blocked");
        Check(await Denied(a=>a.Delete("admin","other","wrong")),"Deletion requires exact login confirmation");
        await Act(a=>a.Invite("admin","invited","invited@example.invalid"));
        string raw;
        using(var db=new OnlineVideosDataContext(options)) {
            var invited=db.Users.Single(u=>u.Email=="invited");Check(invited.IsDisabled&&!invited.IsAdmin&&!invited.EmailConfirmed,"Invitations start disabled and unprivileged");
            var body=db.MailJobs.Single(j=>j.UserId=="invited").Body;raw=Regex.Match(body,"token=([A-F0-9]+)").Groups[1].Value;
            Check(raw.Length==64&&db.AccountTokens.All(t=>t.Id!=raw),"Invitation uses random token stored as hash");
        }
        Check(await Denied(a=>a.Activate(raw,"short")),"Invitation enforces password length");
        await Act(a=>a.Activate(raw,password));
        Check(await Denied(a=>a.Activate(raw,password)),"Invitation token is single use");
        using(var scope=factory.Services.CreateScope()) Check(await scope.ServiceProvider.GetRequiredService<Credentials>().Authenticate("invited",password)!=null,"Activated user can log in");
        Check(await Denied(a=>a.Preferences("invited","invited@example.invalid",true,false,false,false)),"User cannot subscribe to administrator notifications");
        await Act(a=>a.Preferences("admin","admin@example.invalid",true,true,true,true));
        using(var db=new OnlineVideosDataContext(options)){raw=Regex.Match(db.MailJobs.Single(j=>j.UserId=="admin"&&j.Kind=="account").Body,"token=([A-F0-9]+)").Groups[1].Value;}
        await Act(a=>a.Activate(raw,null));
        using(var scope=factory.Services.CreateScope()) {
            var mail=scope.ServiceProvider.GetRequiredService<Notifications>();await mail.Event("reports","1","Fixture");await mail.Event("reports","2","Fixture");
            var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();Check(await db.MailJobs.CountAsync(j=>j.Kind=="reports")==1,"Reports are grouped into one hourly summary");
            await mail.Deliver(CancellationToken.None);Check(await db.MailJobs.AnyAsync(j=>j.Kind=="reports"&&j.Status=="pending"),"Disabled mail transport never sends messages");
        }
        Check(await Denied(a=>a.SetEmail("invited","admin","bad@example.invalid")),"Ordinary users cannot change another account email");
        Check(await Denied(a=>a.SetEmail("admin","invited","bad\r\nBcc: attack@example.invalid")),"Administrator email editing rejects header injection");
        await Act(a=>a.SetEmail("admin","invited","changed@example.invalid"));
        using(var db=new OnlineVideosDataContext(options)) {
            var u=db.Users.Single(x=>x.Email=="invited");Check(u.ContactEmail=="changed@example.invalid"&&!u.EmailConfirmed,"Administrator email change requires new verification");
            Check(db.MailJobs.Any(x=>x.UserId=="invited"&&x.Kind=="account"),"Active account receives a new email confirmation link");
        }
        await Act(a=>a.Edit("admin","other","user",true));await Act(a=>a.SetEmail("admin","other","disabled@example.invalid"));
        using(var db=new OnlineVideosDataContext(options)) Check(db.Users.Single(x=>x.Email=="other").IsDisabled&&!db.AccountTokens.Any(x=>x.UserId=="other"),"Email editing cannot reactivate a disabled account");
        await Act(a=>a.Edit("admin","other","user",false));
        Check(await Denied(a=>a.ChangePassword("invited","wrong",password)),"Password change requires current password");
        await Act(a=>a.ChangePassword("invited",password,"Replacement-Synthetic-Password-2026!"));
        using(var scope=factory.Services.CreateScope()) Check(await scope.ServiceProvider.GetRequiredService<Credentials>().Authenticate("invited",password)==null,"Old password fails after replacement");
        using(var scope=factory.Services.CreateScope()) {
            var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();var owner=await db.Users.FindAsync("owner");
            var review=scope.ServiceProvider.GetRequiredService<SiteReviews>();var id=await review.Stage(owner,"Fixture","<Site name=\"Fixture\" lang=\"en\"><Description>Reviewed content</Description></Site>",null,null,null);
            Check((await db.Sites.FindAsync("Fixture")).Description!="Reviewed content","Site submissions stay private pending review");
            bool blocked=false;try{await review.Decide("invited",id,true);}catch(UnauthorizedAccessException){blocked=true;}Check(blocked,"Only administrators can approve site changes");
        }
        using(var scope=factory.Services.CreateScope()) {
            var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();var id=(await db.SiteChanges.SingleAsync()).Id;await scope.ServiceProvider.GetRequiredService<SiteReviews>().Decide("admin",id,true);
            Check((await db.Sites.FindAsync("Fixture")).Description=="Reviewed content","Administrator review publishes site change");
        }
        await Act(a=>a.Delete("admin","owner","owner"));
        using(var db=new OnlineVideosDataContext(options)) Check(!db.Users.Any(u=>u.Email=="owner")&&db.Sites.Any(x=>x.Name=="Fixture"&&x.OwnerId.StartsWith("__deleted__")),"Deletion preserves contributions under disabled anonymous ownership");
        var page=await browser.GetStringAsync("/Login");var token=Regex.Match(page,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        await browser.PostAsync("/Login?handler=Login",new FormUrlEncodedContent(new Dictionary<string,string>{{"username","admin"},{"password","Changed-Password-Test-Only"},{"__RequestVerificationToken",token}}));
        Check((await browser.GetStringAsync("/Users")).Contains("Invite a user"),"Administrator can open user management");
        Check((int)(await browser.PostAsync("/Users?handler=Edit",new FormUrlEncodedContent(new Dictionary<string,string>{{"target","other"},{"role","admin"}}))).StatusCode==400,"User management retains CSRF protection");
        await Act(a=>a.Edit("admin","other","admin",false));
        using(var db=new OnlineVideosDataContext(options)){var admin=await db.Users.FindAsync("admin");admin.IsAdmin=false;await db.SaveChangesAsync();}
        Check((int)(await browser.GetAsync("/Users")).StatusCode==302,"Role change invalidates an existing browser session");
    }
    {
        var mailOptions=new DbContextOptionsBuilder<OnlineVideosDataContext>().UseSqlite("Data Source="+Path.Combine(root,"mail.db3")).Options;
        await using var db=new OnlineVideosDataContext(mailOptions);await db.Database.EnsureCreatedAsync();
        var u=new User{Email="mail-admin",Password="unused",IsAdmin=true,EmailConfirmed=true,ContactEmail="recipient@example.invalid",NotifyChanges=true};db.Users.Add(u);
        db.SiteChanges.Add(new SiteChange{Name="Mail fixture",OwnerId=u.Email,Xml="<Site />",Created=DateTime.UtcNow});await db.SaveChangesAsync();
        using var listener=new System.Net.Sockets.TcpListener(IPAddress.Loopback,0);listener.Start();
        var port=((IPEndPoint)listener.LocalEndpoint).Port;
        var received=new List<string>();
        var smtpTask=Task.Run(async()=>{using var socket=await listener.AcceptTcpClientAsync();using var stream=socket.GetStream();using var reader=new StreamReader(stream);using var writer=new StreamWriter(stream){AutoFlush=true,NewLine="\r\n"};await writer.WriteLineAsync("220 localhost test");bool data=false;while(await reader.ReadLineAsync() is string line){if(data){received.Add(line);if(line=="."){data=false;await writer.WriteLineAsync("250 accepted");}continue;}if(line.StartsWith("EHLO")||line.StartsWith("HELO"))await writer.WriteLineAsync("250 localhost");else if(line.StartsWith("DATA")){data=true;await writer.WriteLineAsync("354 continue");}else if(line.StartsWith("QUIT")){await writer.WriteLineAsync("221 bye");break;}else await writer.WriteLineAsync("250 ok");}});
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>{{"Mail:Enabled","true"},{"Mail:From","sender@example.invalid"},{"Mail:PublicUrl","https://localhost"},{"Mail:Host","127.0.0.1"},{"Mail:Port",port.ToString()}}).Build();
        var mail=new Notifications(db,config);await mail.Event("changes","1","Pending fixture");await mail.Deliver(CancellationToken.None);await smtpTask.WaitAsync(TimeSpan.FromSeconds(10));
        Check(await db.MailJobs.AnyAsync(j=>j.Status=="sent"&&j.Body==""),"SMTP queue hands off mail and clears delivered body");
        Check(received.Any(x=>x.Contains("recipient@example.invalid"))&&received.Any(x=>x.Contains("https://localhost")),"SMTP test receives expected recipient and HTTPS review link");
        await mail.Event("changes","2","Do not send");u.NotifyChanges=false;await db.SaveChangesAsync();await mail.Deliver(CancellationToken.None);
        Check(await db.MailJobs.AnyAsync(j=>j.EventKey=="changes:2"&&j.Status=="cancelled"),"Unsubscribing cancels queued notifications before SMTP");
    }
    using(var factory=new TestFactory(root,dbpath)) {
        using(var db=new OnlineVideosDataContext(options)){var u=new User{Email="revivable",Password="",IsPublisher=true};u.Password=Credentials.Hash(u,password);db.Users.Add(u);await db.SaveChangesAsync();}
        using var browser=factory.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
        var page=await browser.GetStringAsync("/Login");var token=Regex.Match(page,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        await browser.PostAsync("/Login?handler=Login",new FormUrlEncodedContent(new Dictionary<string,string>{{"username","revivable"},{"password",password},{"__RequestVerificationToken",token}}));
        string initialStamp;
        using(var db=new OnlineVideosDataContext(options))initialStamp=Credentials.Stamp(await db.Users.FindAsync("revivable"));
        foreach(var disabled in new[]{true,false}){using var scope=factory.Services.CreateScope();await scope.ServiceProvider.GetRequiredService<AccountManager>().Edit("other","revivable","publisher",disabled);}
        using(var db=new OnlineVideosDataContext(options))Check(Credentials.Stamp(await db.Users.FindAsync("revivable"))!=initialStamp,"Disable/re-enable rotates the session security version");
        Check(!(await browser.GetStringAsync("/Login")).Contains("Log out"),"Old browser session stays invalid after an account is re-enabled");
        using(var scope=factory.Services.CreateScope()) {
            var trackedDb=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();await trackedDb.Users.FindAsync("other");
            using(var external=new OnlineVideosDataContext(options)){var admin=await external.Users.FindAsync("other");admin.IsAdmin=false;await external.SaveChangesAsync();}
            bool rejected=false;try{await scope.ServiceProvider.GetRequiredService<AccountManager>().Edit("other","revivable","admin",false);}catch(UnauthorizedAccessException){rejected=true;}
            Check(rejected,"Cached administrator entity cannot bypass concurrent revocation");
            using(var external=new OnlineVideosDataContext(options)){var admin=await external.Users.FindAsync("other");admin.IsAdmin=true;await external.SaveChangesAsync();}
        }
        Check((int)(await browser.PostAsync("/api/OnlineVideos/SubmitReport",new StringContent(new string('x',40000),System.Text.Encoding.UTF8,"application/json"))).StatusCode==413,"Oversized report body is rejected before JSON allocation");
        Check((int)(await browser.PostAsync("/Account?handler=Password",new StringContent(new string('x',40000)))).StatusCode==413,"Oversized account form is rejected before model binding");
        Check((int)(await browser.GetAsync("/Reports/SiteReports/does-not-exist")).StatusCode==404,"Missing report page returns 404 instead of a rendering error");
        page=await browser.GetStringAsync("/Login");token=Regex.Match(page,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        await browser.PostAsync("/Login?handler=Login",new FormUrlEncodedContent(new Dictionary<string,string>{{"username","other"},{"password",password},{"__RequestVerificationToken",token}}));
        page=await browser.GetStringAsync("/Reports/SiteReports/Fixture");token=Regex.Match(page,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        var reportResponse=await browser.PostAsync("/Reports/SiteReports/Fixture?handler=SubmitReport",new FormUrlEncodedContent(new Dictionary<string,string>{{"NewReport.Type","Suggestion"},{"NewReport.Date",DateTime.UtcNow.ToString("o")},{"NewReport.Message","Synthetic plain form report"},{"__RequestVerificationToken",token}}));
        Check((int)reportResponse.StatusCode==302,"Report form works without CSP-incompatible AJAX execution");
        Check(!page.Contains("data-ajax=\"true\"")&&!page.Contains("jquery.unobtrusive-ajax.js"),"Unneeded dynamic-code AJAX library is no longer loaded");
        var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);using var first=new HeldBody(release.Task);using var second=new HeldBody(release.Task);
        var a=browser.PostAsync("/api/OnlineVideos/SubmitDll",first);var b=browser.PostAsync("/api/OnlineVideos/SubmitDll",second);
        await Task.WhenAll(first.Started.Task,second.Started.Task).WaitAsync(TimeSpan.FromSeconds(10));await Task.Delay(150);
        try{Check((int)(await browser.PostAsync("/api/OnlineVideos/SubmitSite",new StringContent("{}",System.Text.Encoding.UTF8,"application/json"))).StatusCode==429,"Concurrent heavy uploads share a two-request limit");}
        finally{release.SetResult();await Task.WhenAll(a,b);}
        using(var scope=factory.Services.CreateScope()){
            var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();var env=scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();var owner=await db.Users.FindAsync("revivable");
            db.Sites.Add(new Site{Name="Rollback fixture",OwnerId=owner.Email,Description="Original",Language="en",XML="<Site />",LastUpdated=DateTime.UtcNow});await db.SaveChangesAsync();
            byte[] Png(int width){using var bmp=new SkiaSharp.SKBitmap(width,1);bmp.Erase(SkiaSharp.SKColors.Red);using var encoded=bmp.Encode(SkiaSharp.SKEncodedImageFormat.Png,100);return encoded.ToArray();}
            var iconPath=Path.Combine(env.WebRootPath,"Icons","Rollback fixture.png");var original=new byte[]{1,2,3,4};await File.WriteAllBytesAsync(iconPath,original);
            var review=scope.ServiceProvider.GetRequiredService<SiteReviews>();var id=await review.Stage(owner,"Rollback fixture","<Site name=\"Rollback fixture\" lang=\"en\"><Description>Replacement</Description></Site>",null,Png(1),Png(3));
            var failing=new SiteReviews(db,scope.ServiceProvider.GetRequiredService<AccountManager>(),scope.ServiceProvider.GetRequiredService<PackageReview>(),scope.ServiceProvider.GetRequiredService<Notifications>(),env,new RejectBannerImages());
            bool failed=false;try{await failing.Decide("other",id,true);}catch(System.ComponentModel.DataAnnotations.ValidationException){failed=true;}
            Check(failed&&(await File.ReadAllBytesAsync(iconPath)).SequenceEqual(original),"Failed banner publication restores the previous icon");
            db.ChangeTracker.Clear();Check((await db.Sites.FindAsync("Rollback fixture")).Description=="Original"&&(await db.SiteChanges.FindAsync(id)).Status=="pending","Failed image publication leaves catalogue and review unchanged");
        }
    }
    {
        var opts=new DbContextOptionsBuilder<OnlineVideosDataContext>().UseSqlite("Data Source="+Path.Combine(root,"mail-audit.db3")).Options;
        await using var db=new OnlineVideosDataContext(opts);await db.Database.EnsureCreatedAsync();
        var first=new User{Email="first",Password="unused",IsAdmin=true,EmailConfirmed=true,ContactEmail="shared@example.invalid",NotifyChanges=true};var second=new User{Email="second",Password="unused",IsAdmin=true,EmailConfirmed=true,ContactEmail="shared@example.invalid",NotifyChanges=true};db.Users.AddRange(first,second);db.SiteChanges.Add(new SiteChange{Name="Shared",OwnerId="first",Xml="<Site />",Created=DateTime.UtcNow});await db.SaveChangesAsync();
        using var smtp=new AuditSmtp();var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>{{"Mail:Enabled","true"},{"Mail:From","sender@example.invalid"},{"Mail:PublicUrl","https://localhost"},{"Mail:Host","127.0.0.1"},{"Mail:Port",smtp.Port.ToString()}}).Build();var mail=new Notifications(db,config);
        await mail.Event("changes","1","Shared fixture");Check(await db.MailJobs.CountAsync()==1,"Shared administrator mailbox is deduplicated");
        var recipient=await db.Users.FindAsync((await db.MailJobs.SingleAsync()).UserId);recipient.NotifyChanges=false;await db.SaveChangesAsync();await mail.Deliver(CancellationToken.None);await smtp.Completed.WaitAsync(TimeSpan.FromSeconds(10));
        Check((await db.MailJobs.SingleAsync()).Status=="sent","One administrator opting out does not suppress another subscriber sharing the address");
        var expiredLink=new string('A',64);var currentLink=new string('B',64);db.AccountTokens.Add(new AccountToken{Id=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(currentLink))),UserId="first",Address=first.ContactEmail,Kind="verify",Expires=DateTime.UtcNow.AddHours(1)});
        db.MailJobs.Add(new MailJob{UserId="first",Address=first.ContactEmail,Kind="account",EventKey="obsolete",Subject="Confirm",Body="https://localhost/Activate?token="+expiredLink,Due=DateTime.UtcNow});
        db.MailJobs.Add(new MailJob{UserId="first",Address=first.ContactEmail,Kind="changes",EventKey="interrupted",Subject="Old",Body="old",Due=DateTime.UtcNow,Attempts=5});await db.SaveChangesAsync();
        await mail.Deliver(CancellationToken.None);Check((await db.MailJobs.SingleAsync(j=>j.EventKey=="obsolete")).Status=="cancelled","Obsolete queued account link is cancelled even when a newer token exists");
        Check((await db.MailJobs.AsNoTracking().SingleAsync(j=>j.EventKey=="interrupted")).Status=="failed","Interrupted final mail attempt cannot remain pending forever");
    }

    using(var factory=new TestFactory(root,dbpath)) {
        using(var fixtureDb=new OnlineVideosDataContext(options)){var administrator=await fixtureDb.Users.FindAsync("admin");administrator.IsAdmin=true;administrator.IsDisabled=false;var ordinary=await fixtureDb.Users.FindAsync("other");ordinary.IsAdmin=false;ordinary.IsPublisher=false;await fixtureDb.SaveChangesAsync();}
        using var adminBrowser=factory.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
        using var ordinaryBrowser=factory.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
        using var anonymous=factory.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
        async Task LoginAs(HttpClient browser,string name,string pass){var html=await browser.GetStringAsync("/Login");var csrf=Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;var response=await browser.PostAsync("/Login?handler=Login",new FormUrlEncodedContent(new Dictionary<string,string>{{"username",name},{"password",pass},{"__RequestVerificationToken",csrf}}));if((int)response.StatusCode!=302)throw new Exception("Fixture login failed");}
        await LoginAs(adminBrowser,"admin","Changed-Password-Test-Only");await LoginAs(ordinaryBrowser,"other",password);
        string id,sha;using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();id=await scope.ServiceProvider.GetRequiredService<PackageReview>().Stage(await db.Users.FindAsync("admin"),"Microsoft.Data.Sqlite",File.ReadAllBytes(typeof(Microsoft.Data.Sqlite.SqliteConnection).Assembly.Location));sha=JsonSerializer.Deserialize<PackageReview.Pending>(await File.ReadAllTextAsync(Path.Combine(root,"private",id+".json"))).Sha256;}
        var page=await adminBrowser.GetStringAsync("/Reviews");var token=Regex.Match(page,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Check(page.Contains(id)&&page.Contains(sha)&&Regex.IsMatch(page, "<form[^>]*action=\"[^\"]*handler=ApprovePackage[^\"]*\""),"Administrators can inspect the exact pending package and hash");
        Check((int)(await anonymous.GetAsync("/Reviews")).StatusCode==302,"Anonymous clients cannot open package approval");
        Check((int)(await ordinaryBrowser.GetAsync("/Reviews")).StatusCode==302,"Ordinary users cannot open package approval");
        var form=new Dictionary<string,string>{{"id",id},{"sha256",sha}};
        Check((int)(await adminBrowser.PostAsync("/Reviews?handler=ApprovePackage",new FormUrlEncodedContent(form))).StatusCode==400,"Package approval requires an antiforgery token");
        form["__RequestVerificationToken"]=token;form["sha256"]=new string('0',64);
        var mismatch=await adminBrowser.PostAsync("/Reviews?handler=ApprovePackage",new FormUrlEncodedContent(form));Check((await mismatch.Content.ReadAsStringAsync()).Contains("package has changed")&&File.Exists(Path.Combine(root,"private",id+".dll")),"Package approval rejects a stale or mismatched displayed hash");
        using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();var actor=await db.Users.FindAsync("admin");actor.IsAdmin=false;await db.SaveChangesAsync();bool denied=false;try{await scope.ServiceProvider.GetRequiredService<PackageReview>().Approve(id,"admin",sha);}catch(UnauthorizedAccessException){denied=true;}Check(denied,"Package approval rechecks revoked administrator rights in the transaction");actor.IsAdmin=true;await db.SaveChangesAsync();}
        form["sha256"]=sha;var approved=await adminBrowser.PostAsync("/Reviews?handler=ApprovePackage",new FormUrlEncodedContent(form));
        Check((int)approved.StatusCode==302&&!File.Exists(Path.Combine(root,"private",id+".dll")),"Administrator browser approval publishes and clears private staging");
        using(var db=new OnlineVideosDataContext(options))Check(await db.Dlls.AnyAsync(x=>x.Name=="Microsoft.Data.Sqlite"),"Administrator approval preserves the published catalogue entry");
    }

    using(var factory=new TestFactory(root,dbpath)) {
        using var admin=factory.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
        using var ordinary=factory.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
        using var anonymous=factory.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
        async Task LoginReview(HttpClient browser,string name,string pass){var html=await browser.GetStringAsync("/Login");var csrf=Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;await browser.PostAsync("/Login?handler=Login",new FormUrlEncodedContent(new Dictionary<string,string>{{"username",name},{"password",pass},{"__RequestVerificationToken",csrf}}));}
        await LoginReview(admin,"admin","Changed-Password-Test-Only");await LoginReview(ordinary,"other",password);
        var binary=File.ReadAllBytes(typeof(Microsoft.Data.Sqlite.SqliteConnection).Assembly.Location);string id;
        using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();id=await scope.ServiceProvider.GetRequiredService<PackageReview>().Stage(await db.Users.FindAsync("admin"),"Microsoft.Data.Sqlite",binary);}
        var sha=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(binary));var download="/Reviews?handler=DownloadPackage&id="+id+"&sha256="+sha;
        var result=await admin.GetAsync(download);
        Check(result.IsSuccessStatusCode&&(await result.Content.ReadAsByteArrayAsync()).SequenceEqual(binary),"Administrator inspection download returns the exact staged bytes");
        Check(result.Content.Headers.ContentDisposition?.DispositionType=="attachment"&&result.Headers.CacheControl?.NoStore==true&&result.Headers.GetValues("X-Content-Type-Options").Contains("nosniff"),"Inspection download is an attachment with no-store and nosniff");
        Check((int)(await anonymous.GetAsync(download)).StatusCode==302,"Anonymous inspection download blocked");
        Check((int)(await ordinary.GetAsync(download)).StatusCode==302,"Ordinary user inspection download blocked");
        Check((int)(await admin.GetAsync(download.Replace(sha,new string('0',64)))).StatusCode==400,"Inspection download rejects a mismatched displayed hash");
        Check((int)(await admin.GetAsync("/Reviews?handler=DownloadPackage&id=../outside&sha256="+sha)).StatusCode==400,"Inspection download rejects path traversal identifiers");
        var staged=Path.Combine(root,"private",id+".dll");await File.WriteAllBytesAsync(staged,[1,2,3]);
        Check((int)(await admin.GetAsync(download)).StatusCode==400,"Inspection download rejects modified staged bytes");await File.WriteAllBytesAsync(staged,binary);
        var page=await admin.GetStringAsync("/Reviews");var token=Regex.Match(page,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Check(page.Contains("Download for inspection")&&page.Contains("RejectPackage"),"Package review offers inspection and rejection actions");
        var form=new Dictionary<string,string>{{"id",id},{"sha256",sha}};
        Check((int)(await admin.PostAsync("/Reviews?handler=RejectPackage",new FormUrlEncodedContent(form))).StatusCode==400,"Package rejection requires antiforgery protection");
        Check((int)(await ordinary.PostAsync("/Reviews?handler=RejectPackage",new FormUrlEncodedContent(form))).StatusCode==302,"Ordinary users cannot reject packages");
        form["__RequestVerificationToken"]=token;form["sha256"]=new string('0',64);
        result=await admin.PostAsync("/Reviews?handler=RejectPackage",new FormUrlEncodedContent(form));Check((await result.Content.ReadAsStringAsync()).Contains("package has changed")&&File.Exists(staged),"Rejection binds to the displayed package hash");form["sha256"]=sha;
        using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();var actor=await db.Users.FindAsync("admin");actor.IsAdmin=false;await db.SaveChangesAsync();var service=scope.ServiceProvider.GetRequiredService<PackageReview>();bool deniedDownload=false,deniedReject=false;try{await service.ReadForReview(id,"admin",sha);}catch(UnauthorizedAccessException){deniedDownload=true;}try{await service.Reject(id,"admin",sha);}catch(UnauthorizedAccessException){deniedReject=true;}Check(deniedDownload&&deniedReject,"Inspection and rejection recheck revoked administrator permissions");actor.IsAdmin=true;await db.SaveChangesAsync();}
        var published=Path.Combine(root,"wwwroot","Dlls","Microsoft.Data.Sqlite.dll");var oldPublished=await File.ReadAllBytesAsync(published);
        result=await admin.PostAsync("/Reviews?handler=RejectPackage",new FormUrlEncodedContent(form));
        Check((int)result.StatusCode==302&&!File.Exists(staged)&&!File.Exists(Path.Combine(root,"private",id+".json")),"Administrator rejection removes only private pending package files");
        Check(File.Exists(Path.Combine(root,"private",id+".rejected"))&&(await File.ReadAllTextAsync(Path.Combine(root,"private",id+".rejected"))).Contains("RejectedBy"),"Rejection retains private reviewer and package audit metadata");
        Check((await File.ReadAllBytesAsync(published)).SequenceEqual(oldPublished),"Rejecting an update leaves the published package unchanged");
        Check((int)(await admin.GetAsync(download)).StatusCode==404,"Rejected packages can no longer be downloaded for inspection");
    }
    {
        using var factory=new TestFactory(root,dbpath); using var startup=factory.CreateClient();
        string packageId,sha;int siteId;
        using(var scope=factory.Services.CreateScope()){
            var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();var owner=await db.Users.FindAsync("admin");
            packageId=await scope.ServiceProvider.GetRequiredService<PackageReview>().Stage(owner,"Microsoft.Data.Sqlite",File.ReadAllBytes(typeof(Microsoft.Data.Sqlite.SqliteConnection).Assembly.Location));
            sha=JsonSerializer.Deserialize<PackageReview.Pending>(await File.ReadAllTextAsync(Path.Combine(root,"private",packageId+".json"))).Sha256;
            siteId=await scope.ServiceProvider.GetRequiredService<SiteReviews>().Stage(owner,"Combined fixture","<Site name=\"Combined fixture\" lang=\"en\" />","Microsoft.Data.Sqlite",null,null);
        }
        using(var scope=factory.Services.CreateScope()){
            var review=scope.ServiceProvider.GetRequiredService<SiteReviews>();bool denied=false;
            try{await review.Decide("admin",siteId,true,packageId,new string('0',64));}catch(System.ComponentModel.DataAnnotations.ValidationException){denied=true;}
            Check(denied&&File.Exists(Path.Combine(root,"private",packageId+".json")),"Combined approval rejects a mismatched displayed DLL hash");
        }
        using(var scope=factory.Services.CreateScope()){
            var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();var change=await db.SiteChanges.FindAsync(siteId);change.RequiredDll="Unrelated";await db.SaveChangesAsync();
            bool denied=false;try{await scope.ServiceProvider.GetRequiredService<SiteReviews>().Decide("admin",siteId,true,packageId,sha);}catch(System.ComponentModel.DataAnnotations.ValidationException){denied=true;}
            Check(denied&&File.Exists(Path.Combine(root,"private",packageId+".json")),"Combined approval rejects unrelated DLLs and retains the review");
        }
        using(var scope=factory.Services.CreateScope()){
            var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();var change=await db.SiteChanges.FindAsync(siteId);change.RequiredDll="Microsoft.Data.Sqlite";await db.SaveChangesAsync();
            await scope.ServiceProvider.GetRequiredService<SiteReviews>().Decide("admin",siteId,true,packageId,sha);
            Check((await db.SiteChanges.FindAsync(siteId)).Status=="approved"&&await db.Sites.AnyAsync(x=>x.Name=="Combined fixture"),"Combined approval publishes XML and DLL together");
            Check(!File.Exists(Path.Combine(root,"private",packageId+".json")),"Combined approval consumes the exact package review");
        }
    }
    {
        using var factory=new TestFactory(root,dbpath);using var startup=factory.CreateClient();
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<OnlineVideosDataContext>();var packages=scope.ServiceProvider.GetRequiredService<PackageReview>();var reviews=scope.ServiceProvider.GetRequiredService<SiteReviews>();
        var owner=await db.Users.FindAsync("admin");var id=await packages.Stage(owner,"Microsoft.Data.Sqlite",File.ReadAllBytes(typeof(Microsoft.Data.Sqlite.SqliteConnection).Assembly.Location));
        var sha=JsonSerializer.Deserialize<PackageReview.Pending>(await File.ReadAllTextAsync(Path.Combine(root,"private",id+".json"))).Sha256;
        var site=await reviews.Stage(owner,"Combined rejection","<Site name=\"Combined rejection\" />","Microsoft.Data.Sqlite",null,null);
        var published=Path.Combine(root,"wwwroot","Dlls","Microsoft.Data.Sqlite.dll");var bytes=await File.ReadAllBytesAsync(published);
        await reviews.Decide("admin",site,false,id,sha);
        Check((await db.SiteChanges.FindAsync(site)).Status=="rejected"&&!File.Exists(Path.Combine(root,"private",id+".json")),"Combined rejection rejects both XML and DLL in one action");
        Check((await File.ReadAllBytesAsync(published)).SequenceEqual(bytes)&&!await db.Sites.AnyAsync(x=>x.Name=="Combined rejection"),"Combined rejection preserves published files and creates no site");
    }
    Console.WriteLine($"PASS: {results.Count} security regression checks");
    var output = Environment.GetEnvironmentVariable("OV_SECURITY_RESULTS");
    if (!string.IsNullOrEmpty(output)) File.WriteAllText(output, JsonSerializer.Serialize(new { checks = results.Count, results }, new JsonSerializerOptions { WriteIndented = true }));
}
finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); Directory.Delete(root, true); }

sealed class TestFactory(string root, string dbpath) : WebApplicationFactory<WebServiceCore.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("SecurityTests");
        builder.ConfigureServices(services => services.AddDataProtection().UseEphemeralDataProtectionProvider());
        builder.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../WebServiceCore")));
        var web = Path.Combine(root, "wwwroot");
        foreach (var folder in new[] { "Dlls", "Icons", "Banners" }) Directory.CreateDirectory(Path.Combine(web, folder));
        builder.UseWebRoot(web);
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string> {
            ["ConnectionStrings:OnlineVideos"] = "Data Source=" + dbpath,
            ["Security:PrivateDataPath"] = Path.Combine(root, "private"),
            ["Mail:PublicUrl"] = "https://localhost",
            ["Mail:Enabled"] = "false",
            ["Logging:LogLevel:Default"] = "Warning"
        }));
    }
}

sealed class HeldBody:HttpContent {
 readonly Task release;
 public HeldBody(Task release){this.release=release;Headers.ContentType=new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");}
 public TaskCompletionSource Started {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
 protected override bool TryComputeLength(out long length){length=0;return false;}
 protected override async Task SerializeToStreamAsync(Stream stream,TransportContext context){await stream.WriteAsync(new byte[]{123});await stream.FlushAsync();Started.TrySetResult();await release;await stream.WriteAsync(new byte[]{125});}
}
sealed class RejectBannerImages:IImageService {
 public async Task<string> TrySaveIcon(byte[] data,string path){await File.WriteAllBytesAsync(path,data);return "Icon saved!";}
 public Task<string> TrySaveBanner(byte[] data,string path)=>Task.FromResult("Banner invalid!");
}
sealed class AuditSmtp:IDisposable {
 readonly System.Net.Sockets.TcpListener listener=new(IPAddress.Loopback,0);
 public int Port=>((IPEndPoint)listener.LocalEndpoint).Port;
 public Task Completed {get;}
 public AuditSmtp(){listener.Start();Completed=Task.Run(async()=>{using var socket=await listener.AcceptTcpClientAsync();using var stream=socket.GetStream();using var reader=new StreamReader(stream);using var writer=new StreamWriter(stream){AutoFlush=true,NewLine="\r\n"};await writer.WriteLineAsync("220 localhost test");bool data=false;while(await reader.ReadLineAsync() is string line){if(data){if(line=="."){data=false;await writer.WriteLineAsync("250 accepted");}continue;}if(line.StartsWith("EHLO")||line.StartsWith("HELO"))await writer.WriteLineAsync("250 localhost");else if(line.StartsWith("DATA")){data=true;await writer.WriteLineAsync("354 continue");}else if(line.StartsWith("QUIT")){await writer.WriteLineAsync("221 bye");break;}else await writer.WriteLineAsync("250 ok");}});}
 public void Dispose()=>listener.Stop();
}

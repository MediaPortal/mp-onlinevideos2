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
        var owner = new User { Email = "owner", Password = password, IsAdmin = false };
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
            ["Security:Publishers:0"] = "owner",
            ["Logging:LogLevel:Default"] = "Warning"
        }));
    }
}


using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using WebServiceCore.Models;
using WebServiceCore.Models.Dto;
using WebServiceCore.Models.Entities;
using WebServiceCore.Services;

var checks = 0;
void Check(bool result, string label)
{
    if (!result) throw new Exception(label);
    checks++;
}
byte[] Png(int width, int height)
{
    using var image = new SKBitmap(width, height);
    image.Erase(SKColors.Orange);
    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
    return data.ToArray();
}
var directory = Path.Combine(Path.GetTempPath(), "ov-net10-" + Guid.NewGuid());
Directory.CreateDirectory(directory);
try
{
    var service = new ImageService();
    var icon = Png(32, 32);
    var banner = Png(96, 32);
    var iconPath = Path.Combine(directory, "icon.png");
    var bannerPath = Path.Combine(directory, "banner.png");
    Check(await service.TrySaveIcon(icon, iconPath) == "Icon saved!", "Square PNG accepted");
    Check(File.ReadAllBytes(iconPath).SequenceEqual(icon), "Icon bytes preserved");
    Check(await service.TrySaveBanner(banner, bannerPath) == "Banner saved!", "3:1 PNG accepted");
    Check(File.ReadAllBytes(bannerPath).SequenceEqual(banner), "Banner bytes preserved");
    var rejected = Path.Combine(directory, "rejected.png");
    Check(await service.TrySaveIcon(banner, rejected) == "Icon not saved! Height must be equal to width!", "Rectangular icon rejected");
    Check(await service.TrySaveBanner(icon, rejected) == "Banner not saved! Width must be 3 times height!", "Square banner rejected");
    Check(await service.TrySaveIcon([1, 2, 3], rejected) == "Icon invalid!", "Invalid icon rejected");
    Check(await service.TrySaveBanner([1, 2, 3], rejected) == "Banner invalid!", "Invalid banner rejected");
    using (var bitmap = new SKBitmap(32, 32))
    using (var jpeg = bitmap.Encode(SKEncodedImageFormat.Jpeg, 90))
    {
        Check(await service.TrySaveIcon(jpeg.ToArray(), rejected) == "Icon not saved! Must be PNG!", "JPEG icon rejected");
        Check(await service.TrySaveBanner(jpeg.ToArray(), rejected) == "Banner not saved! Must be PNG!", "JPEG banner rejected");
    }
    Check(!File.Exists(rejected), "Rejected images never written");
    using var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();
    using var context = new OnlineVideosDataContext(new DbContextOptionsBuilder<OnlineVideosDataContext>().UseSqlite(connection).Options);
    context.Database.EnsureCreated();
    var user = new User
    {
        Email = "compatibility@example.invalid",
        Password = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24)),
        IsAdmin = false
    };
    context.Users.Add(user);
    context.Sites.Add(new Site
    {
        Name = "Compatibility",
        XML = "<Site />",
        Language = "en",
        Description = "Test only",
        Owner = user,
        LastUpdated = DateTime.UtcNow
    });
    await context.SaveChangesAsync();
    Check(await context.Users.CountAsync() == 1, "SQLite user insert");
    Check((await context.Sites.AsNoTrackingWithIdentityResolution().Select(s => s.ToGetSiteDto()).SingleAsync()).OwnerId == user.Email, "Existing API projection");
    var site = await context.Sites.SingleAsync();
    site.Description = "Updated";
    await context.SaveChangesAsync();
    Check(await context.Sites.AnyAsync(s => s.Description == "Updated"), "SQLite update");
    context.Sites.Remove(site);
    await context.SaveChangesAsync();
    Check(await context.Sites.CountAsync() == 0, "SQLite delete");
    Console.WriteLine($"PASS: {checks} image and SQLite compatibility checks on {System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}");
}
finally
{
    Directory.Delete(directory, true);
}

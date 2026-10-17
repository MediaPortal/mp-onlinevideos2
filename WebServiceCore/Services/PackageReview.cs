using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text.Json;
using WebServiceCore.Models;
using WebServiceCore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebServiceCore.Services;

// Remote uploads only stage packages. Publication requires explicit administrator or local operator approval.
public sealed class PackageReview(IWebHostEnvironment env, IConfiguration config, OnlineVideosDataContext db, Notifications notifications = null)
{
    public string Root => Path.GetFullPath(config["Security:PrivateDataPath"] ?? Path.Combine(env.ContentRootPath, "private-data"));
    public bool IsPublisher(User user) => !user.IsDisabled && (user.IsAdmin || user.IsPublisher);
    public sealed record Pending(string Name, string Owner, string Sha256, DateTime Submitted);
    FileStream Lock()
    {
        var web = Path.GetFullPath(env.WebRootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if ((Root + Path.DirectorySeparatorChar).StartsWith(web, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidOperationException("Private data must be outside wwwroot.");
        Directory.CreateDirectory(Root);
        return new FileStream(Path.Combine(Root, "publish.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }
    static void Validate(byte[] bytes, string expectedName)
    {
        if (!InputRules.SafeName(expectedName) || bytes.Length == 0 || bytes.Length > InputRules.MaxDllBytes)
            throw new ValidationException("Invalid package name or size.");
        try
        {
            using var stream = new MemoryStream(bytes, false);
            using var pe = new PEReader(stream);
            if (!pe.HasMetadata || pe.PEHeaders.CorHeader == null || !pe.PEHeaders.CoffHeader.Characteristics.HasFlag(Characteristics.Dll))
                throw new BadImageFormatException();
            var metadata = pe.GetMetadataReader();
            if (!metadata.IsAssembly || metadata.GetString(metadata.GetAssemblyDefinition().Name) != expectedName)
                throw new BadImageFormatException();
        }
        catch (Exception ex) when (ex is BadImageFormatException or InvalidOperationException or IOException)
        { throw new ValidationException("A managed DLL with a matching assembly name is required."); }
    }
    public async Task<string> Stage(User owner, string name, byte[] data)
    {
        owner=await db.Users.AsNoTracking().SingleOrDefaultAsync(u=>u.Email==owner.Email);
        if (owner==null || !IsPublisher(owner)) throw new ValidationException("Publisher approval required.");
        Validate(data, name);
        using var gate = Lock();
        if (new DirectoryInfo(Root).EnumerateFiles().Sum(f => f.Length) + data.Length > 128L * 1024 * 1024)
            throw new ValidationException("Review queue quota exceeded.");
        var existing = await db.Dlls.FindAsync(name);
        if (existing != null && existing.OwnerId != owner.Email && !owner.IsAdmin)
            throw new ValidationException("Only the owner or an administrator can replace this package.");
        var id = Guid.NewGuid().ToString("N");
        var binary = Path.Combine(Root, id + ".dll");
        try
        {
            await File.WriteAllBytesAsync(binary, data);
            var info = new Pending(name, owner.Email, Convert.ToHexString(SHA256.HashData(data)), DateTime.UtcNow);
            await File.WriteAllTextAsync(Path.Combine(Root, id + ".json"), JsonSerializer.Serialize(info));
        }
        catch { File.Delete(binary); throw; }
        return id;
    }
    public async Task<(string Name, byte[] Bytes)> ReadForReview(string id, string administrator, string expectedSha256)
    {
        if (!Guid.TryParseExact(id, "N", out _)) throw new ValidationException("Invalid review identifier.");
        using var gate = Lock();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var actor = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == administrator);
        if (actor == null || !actor.IsAdmin || actor.IsDisabled) throw new UnauthorizedAccessException();
        var info = JsonSerializer.Deserialize<Pending>(await File.ReadAllTextAsync(Path.Combine(Root, id + ".json")))
            ?? throw new ValidationException("Missing review metadata.");
        if (!InputRules.SafeName(info.Name) || !string.Equals(info.Sha256, expectedSha256, StringComparison.Ordinal))
            throw new ValidationException("The package has changed. Reload the review before downloading it.");
        var path = Path.Combine(Root, id + ".dll");
        if (new FileInfo(path).Length > InputRules.MaxDllBytes) throw new ValidationException("Package exceeds review size limit.");
        var bytes = await File.ReadAllBytesAsync(path);
        if (Convert.ToHexString(SHA256.HashData(bytes)) != info.Sha256) throw new ValidationException("Package hash changed.");
        return (info.Name, bytes);
    }
    public async Task Reject(string id, string administrator, string expectedSha256, Func<Pending, Task> rejectRelatedSite = null)
    {
        if (!Guid.TryParseExact(id, "N", out _)) throw new ValidationException("Invalid review identifier.");
        using var gate = Lock();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var actor = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == administrator);
        if (actor == null || !actor.IsAdmin || actor.IsDisabled) throw new UnauthorizedAccessException();
        var metadata = Path.Combine(Root, id + ".json");
        var info = JsonSerializer.Deserialize<Pending>(await File.ReadAllTextAsync(metadata))
            ?? throw new ValidationException("Missing review metadata.");
        if (!string.Equals(info.Sha256, expectedSha256, StringComparison.Ordinal))
            throw new ValidationException("The package has changed. Reload the review before rejecting it.");
        var backup = Path.Combine(Root, id + ".rejecting");
        var audit = Path.Combine(Root, id + ".rejected");
        File.Move(metadata, backup);
        try
        {
            if (rejectRelatedSite != null) await rejectRelatedSite(info);
            if (notifications != null) await notifications.Event("own", "rejected:" + id, "Package rejected: " + info.Name, info.Owner);
            await File.WriteAllTextAsync(audit, JsonSerializer.Serialize(new { Package = info, RejectedBy = administrator, RejectedAt = DateTime.UtcNow }));
            await transaction.CommitAsync();
        }
        catch
        {
            File.Move(backup, metadata);
            File.Delete(audit);
            throw;
        }
        File.Delete(backup);
        File.Delete(Path.Combine(Root, id + ".dll"));
    }
    public async Task Approve(string id, string administrator = null, string expectedSha256 = null, Func<Pending, Task> publishRelatedSite = null)
    {
        if (!Guid.TryParseExact(id, "N", out _)) throw new ValidationException("Invalid review identifier.");
        using var gate = Lock();
        var info = JsonSerializer.Deserialize<Pending>(await File.ReadAllTextAsync(Path.Combine(Root, id + ".json")))
            ?? throw new ValidationException("Missing review metadata.");
        var bytes = await File.ReadAllBytesAsync(Path.Combine(Root, id + ".dll"));
        Validate(bytes, info.Name);
        if (Convert.ToHexString(SHA256.HashData(bytes)) != info.Sha256) throw new ValidationException("Package hash changed.");
        var destination = InputRules.AssetPath(env.WebRootPath, "Dlls", info.Name, ".dll");
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        if (new DirectoryInfo(Path.GetDirectoryName(destination)!).EnumerateFiles().Sum(f => f.Length) + bytes.Length > 512L * 1024 * 1024)
            throw new ValidationException("Published package quota exceeded.");
        await using var transaction = await db.Database.BeginTransactionAsync();
        if (administrator != null)
        {
            var actor = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == administrator);
            if (actor == null || !actor.IsAdmin || actor.IsDisabled) throw new UnauthorizedAccessException();
            if (!string.Equals(info.Sha256, expectedSha256, StringComparison.Ordinal))
                throw new ValidationException("The package has changed. Reload the review before approving it.");
        }
        var owner=await db.Users.AsNoTracking().SingleOrDefaultAsync(u=>u.Email==info.Owner);
        if(owner==null || !IsPublisher(owner)) throw new ValidationException("Publisher approval has been revoked.");
        var dll = await db.Dlls.FindAsync(info.Name);
        if (dll != null && dll.OwnerId != owner.Email && !owner.IsAdmin) throw new ValidationException("Package ownership changed.");
        if (dll == null) { dll = new Dll { Name = info.Name, OwnerId = owner.Email }; db.Dlls.Add(dll); }
        dll.MD5 = Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant(); // Legacy protocol checksum only.
        dll.LastUpdated = DateTime.UtcNow;
        var temporary = destination + "." + id + ".tmp";
        var backup = destination + "." + id + ".bak";
        var hadOriginal = File.Exists(destination);
        if (hadOriginal) File.Copy(destination, backup, false);
        var replaced = false;
        try
        {
            await File.WriteAllBytesAsync(temporary, bytes);
            File.Move(temporary, destination, true);
            replaced = true;
            await db.SaveChangesAsync();
            if (publishRelatedSite != null) await publishRelatedSite(info);
            if(notifications != null) await notifications.Event("own", id, "Package approved: " + info.Name, owner.Email);
            await transaction.CommitAsync();
        }
        catch
        {
            if (replaced) { if (hadOriginal) File.Move(backup, destination, true); else File.Delete(destination); }
            throw;
        }
        finally { File.Delete(temporary); }
        File.Delete(backup);
        File.Delete(Path.Combine(Root, id + ".json"));
        File.Delete(Path.Combine(Root, id + ".dll"));
    }
}

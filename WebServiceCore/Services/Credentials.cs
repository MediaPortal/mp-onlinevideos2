using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using WebServiceCore.Models;
using WebServiceCore.Models.Entities;

namespace WebServiceCore.Services;

public sealed class Credentials(OnlineVideosDataContext db, IMemoryCache cache)
{
    const string Prefix = "OVH1:";
    static readonly PasswordHasher<User> Hasher = new(Microsoft.Extensions.Options.Options.Create(
        new PasswordHasherOptions { IterationCount = 210000 }));
    static readonly object AttemptsLock = new();
    public static string Hash(User user, string password) => Prefix + Hasher.HashPassword(user, password);
    public static string Stamp(User user) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(user.Password + "|" + user.IsAdmin + "|" + user.IsPublisher + "|" + user.IsDisabled + "|" + user.SecurityVersion)));

    public static bool Verify(User user, string password) {
        try { return password != null && user.Password.StartsWith(Prefix, StringComparison.Ordinal) && Hasher.VerifyHashedPassword(user,user.Password[Prefix.Length..],password) != PasswordVerificationResult.Failed; }
        catch(FormatException) { return false; }
    }
    public static async Task Migrate(OnlineVideosDataContext db)
    {
        // One-way, transactional conversion of the legacy plaintext column. No plaintext login fallback.
        await using var transaction = await db.Database.BeginTransactionAsync();
        await foreach (var user in db.Users.Where(u=>!u.Password.StartsWith(Prefix)).AsAsyncEnumerable())
            if (!user.Password.StartsWith(Prefix, StringComparison.Ordinal))
                user.Password = Hash(user, user.Password);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<User?> Authenticate(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 254 || string.IsNullOrEmpty(password) || password.Length > 256)
            return null;
        // The same budget applies to API and browser login. Failed attempts do not extend the lock indefinitely.
        var key = "auth:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email.ToUpperInvariant())));
        lock (AttemptsLock)
        {
            var attempts = cache.Get<int>(key);
            if (attempts >= 5) return null;
            cache.Set(key, attempts + 1, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15), Size = 1 });
        }
        var user = await db.Users.FindAsync(email);
        // Unknown users still incur one password verification to reduce timing-based enumeration.
        var hash = user?.Password;
        var valid = false;
        try
        {
            valid = Hasher.VerifyHashedPassword(user ?? new User(),
                hash?.StartsWith(Prefix, StringComparison.Ordinal) == true ? hash[Prefix.Length..] : DummyHash, password)
                != PasswordVerificationResult.Failed && user != null && hash!.StartsWith(Prefix, StringComparison.Ordinal);
        }
        catch (FormatException) { }
        if (!valid || user!.IsDisabled) return null;
        lock (AttemptsLock) cache.Remove(key);
        return user;
    }
    static readonly string DummyHash = Hasher.HashPassword(new User(), "dummy-unusable-" + Guid.NewGuid());
}

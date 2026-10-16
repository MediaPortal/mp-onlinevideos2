#nullable enable
using System.ComponentModel.DataAnnotations;
namespace WebServiceCore.Services;
public static class InputRules
{
    public const int MaxDllBytes = 8 * 1024 * 1024;
    public const int MaxImageBytes = 2 * 1024 * 1024;
    public const long MaxBodyBytes = 16 * 1024 * 1024;
    public static bool SafeName(string? name) => !string.IsNullOrWhiteSpace(name) && name.Length <= 120
        && name != "." && name != ".." && name == name.Trim() && !name.EndsWith('.')
        && !name.Any(c => char.IsControl(c) || "/\\:*?\"<>|".Contains(c));
    public static string AssetPath(string root, string directory, string name, string extension)
    {
        if (!SafeName(name)) throw new ValidationException("Invalid asset name.");
        var parent = Path.GetFullPath(Path.Combine(root, directory));
        var full = Path.GetFullPath(Path.Combine(parent, name + extension));
        if (!full.StartsWith(parent + Path.DirectorySeparatorChar, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new ValidationException("Invalid asset path.");
        return full;
    }
}

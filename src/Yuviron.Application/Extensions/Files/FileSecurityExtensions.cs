using System;
using System.Linq;

namespace Yuviron.Application.Extensions;

public static class FileSecurityExtensions
{
    // Жестко зашитые в код папки, которые НИКОГДА не должны быть публичными,
    // даже если кто-то случайно пропишет их в appsettings.json.
    private static readonly string[] StrictlyForbiddenPrefixes = 
    { 
        "tracks/", 
    };

    /// <summary>
    /// Проверяет, находится ли ключ хранилища в разрешенном публичном списке папок,
    /// и гарантирует, что это не премиум-контент.
    /// </summary>
    public static bool IsPublicResource(this string storageKey, string[] allowedFolders)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return false;
        }

        bool isForbidden = StrictlyForbiddenPrefixes.Any(folder => 
            storageKey.StartsWith(folder, StringComparison.OrdinalIgnoreCase));

        if (isForbidden)
        {
            return false; 
        }

        if (allowedFolders == null || allowedFolders.Length == 0)
        {
            return false;
        }

        return allowedFolders.Any(folder => 
            storageKey.StartsWith(folder, StringComparison.OrdinalIgnoreCase));
    }
}

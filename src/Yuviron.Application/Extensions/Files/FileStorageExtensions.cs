using System.Diagnostics.CodeAnalysis;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Extensions;

public static class FileStorageExtensions
{
    public static async Task<string?> MoveIfTempAsync(
        this IFileStorageService storage,
        string? path,
        string destinationFolder,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("temp/"))
        {
            return path;
        }

        return await storage.MoveAsync(path!, destinationFolder, ct);
    }
    [return: NotNullIfNotNull(nameof(path))]
    public static string? PredictDestinationPath(string? path, string destinationFolder)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("temp/"))
        {
            return path;
        }

        var fileName = Path.GetFileNameWithoutExtension(path);
        return Path.Combine(destinationFolder, fileName).Replace("\\", "/");
    }
}

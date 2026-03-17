using Microsoft.Extensions.Configuration;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRoot;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _storageRoot = configuration["FILE_STORAGE_ROOT"] 
                       ?? Environment.GetEnvironmentVariable("FILE_STORAGE_ROOT") 
                       ?? "/var/yuviron/storage";
        
        _storageRoot = Path.GetFullPath(_storageRoot);
    }

    public async Task<string> UploadAsync(Stream stream, string folder, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var targetDirectory = GetValidatedFullPath(folder);

        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        var fullFilePath = Path.Combine(targetDirectory, uniqueFileName);

        using (var fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
    }

    public Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey)) return Task.CompletedTask;

        var fullPath = GetValidatedFullPath(fileKey);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetValidatedFullPath(string subPath)
    {
        if (subPath.Contains(".."))
        {
            throw new ArgumentException("Directory traversal characters ('..') are not allowed.");
        }

        var combinedPath = Path.Combine(_storageRoot, subPath);
        var fullPath = Path.GetFullPath(combinedPath);

        if (!fullPath.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Access to the requested path is denied.");
        }

        return fullPath;
    }
}
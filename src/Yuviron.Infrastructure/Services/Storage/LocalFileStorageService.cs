using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Infrastructure.Utilities;

namespace Yuviron.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRoot;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IConfiguration configuration, 
        ILogger<LocalFileStorageService> logger)
    {
        _storageRoot = configuration["FILE_STORAGE_ROOT"] 
                       ?? Environment.GetEnvironmentVariable("FILE_STORAGE_ROOT") 
                       ?? "/var/yuviron/storage";
        _storageRoot = Path.GetFullPath(_storageRoot);
        _logger = logger;
    }

    public async Task<string> UploadAsync(Stream stream, string folder, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var targetDirectory = StoragePathValidator.GetValidatedFullPath(_storageRoot, folder);
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        
        if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);

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

        try
        {
            var fullPath = StoragePathValidator.GetValidatedFullPath(_storageRoot, fileKey);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "File {FileKey} is locked and cannot be deleted right now.", fileKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting file {FileKey}.", fileKey);
            throw;
        }

        return Task.CompletedTask;
    }

    public Task<string> MoveAsync(string sourceFileKey, string destinationFolder, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceFileKey) || !sourceFileKey.StartsWith("temp/"))
        {
            return Task.FromResult(sourceFileKey); 
        }

        var sourcePath = StoragePathValidator.GetValidatedFullPath(_storageRoot, sourceFileKey);
        var targetDirectory = StoragePathValidator.GetValidatedFullPath(_storageRoot, destinationFolder);
        
        var fileName = Path.GetFileName(sourceFileKey);
        var targetFilePath = Path.Combine(targetDirectory, fileName);
        var finalRelativePath = Path.Combine(destinationFolder, fileName).Replace("\\", "/");

        if (!File.Exists(sourcePath))
        {
            if (File.Exists(targetFilePath))
            {
                return Task.FromResult(finalRelativePath);
            }

            throw new FileNotFoundException($"Source temp file not found for moving: {sourcePath}");
        }

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        File.Move(sourcePath, targetFilePath, overwrite: true);

        return Task.FromResult(finalRelativePath);
    }
    
    public Task<Stream> GetFileStreamAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            throw new ArgumentException("File key cannot be null or empty.", nameof(fileKey));
        }

        var fullPath = StoragePathValidator.GetValidatedFullPath(_storageRoot, fileKey);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {fullPath}");
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

        return Task.FromResult(stream);
    }
    
    public Task DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(directoryPath)) return Task.CompletedTask;

        try
        {
            var fullPath = StoragePathValidator.GetValidatedFullPath(_storageRoot, directoryPath); 
            
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, recursive: true);
                _logger.LogInformation("The directory and all its files have been successfully deleted: {Path}", fullPath);
            }
            else
            {
                _logger.LogWarning("Directory {Path} not found, deletion skipped.", fullPath);
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "The directory {DirectoryPath} is locked and cannot be deleted right now.", directoryPath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting the directory. {DirectoryPath}.", directoryPath);
            throw;
        }

        return Task.CompletedTask;
    }
}
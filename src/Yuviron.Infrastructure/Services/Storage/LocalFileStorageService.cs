using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Infrastructure.Utilities;

namespace Yuviron.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRoot;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IOptions<StorageOptions> options, 
        ILogger<LocalFileStorageService> logger)
    {
        var configuredRoot = Environment.GetEnvironmentVariable("FILE_STORAGE_ROOT") ?? options.Value.RootPath;
        _storageRoot = Path.GetFullPath(configuredRoot);
        _logger = logger;
    }

    public async Task<string> UploadAsync(Stream stream, string folder, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var targetDirectory = StoragePathValidator.GetValidatedFullPath(_storageRoot, folder);
        var uniqueFileName = fileName;
        
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
            cancellationToken.ThrowIfCancellationRequested();
            var fullPath = StoragePathValidator.GetValidatedFullPath(_storageRoot, fileKey);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to delete file {FileKey}.", fileKey);
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

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);

            File.Move(sourcePath, targetFilePath, overwrite: true);

            return Task.FromResult(finalRelativePath);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to move file {SourceFileKey} to {DestinationFolder}.", sourceFileKey, destinationFolder);
            throw;
        }
    }
    
    public async Task<Stream?> GetFileStreamAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey)) throw new ArgumentException("File key cannot be null or empty.", nameof(fileKey));

        var fullPath = StoragePathValidator.GetValidatedFullPath(_storageRoot, fileKey);

        if (!await ExistsAsync(fileKey, cancellationToken)) return null; 

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
    }
    
    public Task DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(directoryPath)) return Task.CompletedTask;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fullPath = StoragePathValidator.GetValidatedFullPath(_storageRoot, directoryPath); 
            
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, recursive: true);
                _logger.LogInformation("The directory and all its files have been successfully deleted: {Path}", fullPath);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to delete directory {DirectoryPath}.", directoryPath);
            throw;
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey)) return Task.FromResult(false);
        
        var fullPath = StoragePathValidator.GetValidatedFullPath(_storageRoot, fileKey);
        return Task.FromResult(File.Exists(fullPath));
    }
}
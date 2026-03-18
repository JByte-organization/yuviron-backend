using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Infrastructure.BackgroundJobs;

public class TempFilesCleanupJob : BackgroundService
{
    private readonly ILogger<TempFilesCleanupJob> _logger;
    private readonly string _tempFolderPath;
    
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12);
    
    private readonly TimeSpan _expirationAge = TimeSpan.FromHours(24);

    public TempFilesCleanupJob(
        ILogger<TempFilesCleanupJob> logger, 
        IConfiguration configuration)
    {
        _logger = logger;
        
        var storageRoot = configuration["FILE_STORAGE_ROOT"] 
                          ?? Environment.GetEnvironmentVariable("FILE_STORAGE_ROOT") 
                          ?? "/var/yuviron/storage";
                          
        _tempFolderPath = Path.Combine(storageRoot, "temp");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Temp Files Cleanup Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                CleanupOldFiles();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up temp files.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private void CleanupOldFiles()
    {
        if (!Directory.Exists(_tempFolderPath)) return;

        var files = Directory.GetFiles(_tempFolderPath);
        var now = DateTime.UtcNow;
        var deletedCount = 0;

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            
            if (now - fileInfo.CreationTimeUtc > _expirationAge)
            {
                fileInfo.Delete();
                deletedCount++;
            }
        }

        if (deletedCount > 0)
        {
            _logger.LogInformation("Cleaned up {Count} old files from temp folder.", deletedCount);
        }
    }
}
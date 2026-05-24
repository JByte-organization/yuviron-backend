using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Infrastructure.BackgroundJobs;

public class TempFilesCleanupJob : BackgroundService
{
    private readonly ILogger<TempFilesCleanupJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory; // Используем фабрику скоупов
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Можно чаще, теперь это дешево
    private readonly TimeSpan _expirationAge = TimeSpan.FromHours(24);

    public TempFilesCleanupJob(
        ILogger<TempFilesCleanupJob> logger, 
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Temp Files Cleanup Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupFiles(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up temp files.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CleanupFiles(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        var threshold = DateTime.UtcNow.Add(-_expirationAge);

        // Ищем записи, которые устарели
        var expiredFiles = await context.FileMetadata
            .Where(f => f.IsTemporary && f.CreatedAt < threshold)
            .ToListAsync(stoppingToken);

        if (!expiredFiles.Any()) return;

        var deletedCount = 0;
        foreach (var fileMeta in expiredFiles)
        {
            try
            {
                // 1. Сначала удаляем физически
                await storage.DeleteAsync(fileMeta.CurrentStorageKey, stoppingToken);
                
                // 2. Удаляем запись из БД
                context.FileMetadata.Remove(fileMeta);
                deletedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up file {FileId}", fileMeta.Id);
            }
        }

        await context.SaveChangesAsync(stoppingToken);
        
        if (deletedCount > 0)
        {
            _logger.LogInformation("Cleaned up {Count} files from database and storage.", deletedCount);
        }
    }
}
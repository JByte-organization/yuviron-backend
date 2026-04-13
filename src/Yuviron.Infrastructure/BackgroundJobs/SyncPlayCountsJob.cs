using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Yuviron.Application.Abstractions;

namespace Yuviron.Infrastructure.BackgroundJobs;

public class SyncPlayCountsJob : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncPlayCountsJob> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);

    public SyncPlayCountsJob(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory, ILogger<SyncPlayCountsJob> logger)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка синхронизации счетчиков");
            }
            await Task.Delay(_syncInterval, stoppingToken);
        }
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        
        // Достаем ID треков и артистов, которые изменились
        var trackIdsRaw = await db.SetPopAsync("dirty_counters:tracks", 1000);
        var artistIdsRaw = await db.SetPopAsync("dirty_counters:artists", 1000);

        if (trackIdsRaw.Length == 0 && artistIdsRaw.Length == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Обновляем треки
        foreach (var idRaw in trackIdsRaw)
        {
            var trackId = Guid.Parse(idRaw.ToString());
            var count = (long)await db.StringGetDeleteAsync($"track:{trackId}:plays");
            if (count > 0)
            {
                var track = await context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId, ct);
                track?.AddPlays(count);
            }
        }

        // Обновляем артистов
        foreach (var idRaw in artistIdsRaw)
        {
            var artistId = Guid.Parse(idRaw.ToString());
            var count = (long)await db.StringGetDeleteAsync($"artist:{artistId}:plays");
            if (count > 0)
            {
                var artist = await context.Artists.FirstOrDefaultAsync(a => a.Id == artistId, ct);
                artist?.AddPlays(count);
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
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
        
        var trackIdsRaw = await db.SetMembersAsync("dirty_counters:tracks");
        var artistIdsRaw = await db.SetMembersAsync("dirty_counters:artists");

        if (trackIdsRaw.Length == 0 && artistIdsRaw.Length == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var tracksToReset = new Dictionary<RedisKey, long>();
        var artistsToReset = new Dictionary<RedisKey, long>();

        
        foreach (var idRaw in trackIdsRaw)
        {
            var trackId = Guid.Parse(idRaw.ToString());
            var redisKey = new RedisKey($"track:{trackId}:plays");
            
            var countRaw = await db.StringGetSetAsync(redisKey, 0); 
            if (!countRaw.HasValue) continue;
            
            long count = (long)countRaw;
            if (count > 0)
            {
                var track = await context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId, ct);
                if (track != null)
                {
                    track.AddPlays(count);
                    tracksToReset[redisKey] = count; 
                }
            }
        }

        foreach (var idRaw in artistIdsRaw)
        {
            var artistId = Guid.Parse(idRaw.ToString());
            var redisKey = new RedisKey($"artist:{artistId}:plays");
            
            var countRaw = await db.StringGetSetAsync(redisKey, 0);
            if (!countRaw.HasValue) continue;

            long count = (long)countRaw;
            if (count > 0)
            {
                var artist = await context.Artists.FirstOrDefaultAsync(a => a.Id == artistId, ct);
                if (artist != null)
                {
                    artist.AddPlays(count);
                    artistsToReset[redisKey] = count;
                }
            }
        }

        try
        {
            await context.SaveChangesAsync(ct);
            
            if (trackIdsRaw.Length > 0)
            {
                await db.SetRemoveAsync("dirty_counters:tracks", trackIdsRaw);
            }
            if (artistIdsRaw.Length > 0)
            {
                await db.SetRemoveAsync("dirty_counters:artists", artistIdsRaw);
            }
        }
        catch (Exception)
        {
            foreach (var kvp in tracksToReset)
            {
                await db.StringIncrementAsync(kvp.Key, kvp.Value);
            }
            foreach (var kvp in artistsToReset)
            {
                await db.StringIncrementAsync(kvp.Key, kvp.Value);
            }
            
            throw;
        }
    }
}
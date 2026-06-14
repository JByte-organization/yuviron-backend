using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Yuviron.Application.Features.Admin.Analytics.Commands.SyncAnalytics;

namespace Yuviron.Infrastructure.BackgroundJobs;

public class SyncPlayCountsJob : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncPlayCountsJob> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);
    private const int BatchSize = 1000;

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
            try { await SyncAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Error syncing counters"); }
            await Task.Delay(_syncInterval, stoppingToken);
        }
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        
        // Use batching for track IDs to avoid OOM with large sets
        var trackIdsRaw = await db.SetMembersAsync("dirty_counters:tracks");
        var artistIdsRaw = await db.SetMembersAsync("dirty_counters:artists");

        if (trackIdsRaw.Length == 0 && artistIdsRaw.Length == 0) return;

        // Process in batches
        for (int i = 0; i < trackIdsRaw.Length; i += BatchSize)
        {
            var batch = trackIdsRaw.Skip(i).Take(BatchSize).ToList();
            await ProcessTrackBatchAsync(db, batch, ct);
        }

        for (int i = 0; i < artistIdsRaw.Length; i += BatchSize)
        {
            var batch = artistIdsRaw.Skip(i).Take(BatchSize).ToList();
            await ProcessArtistBatchAsync(db, batch, ct);
        }
    }

    private async Task ProcessTrackBatchAsync(IDatabase db, List<RedisValue> batch, CancellationToken ct)
    {
        var tracksToSync = new Dictionary<Guid, long>();
        foreach (var idRaw in batch)
        {
            var trackId = Guid.Parse(idRaw.ToString());
            var countRaw = await db.StringGetAsync("track:" + trackId + ":plays"); 
            if (countRaw.HasValue && (long)countRaw > 0)
                tracksToSync[trackId] = (long)countRaw;
        }

        if (tracksToSync.Any()) await DispatchSyncAsync(tracksToSync, new Dictionary<Guid, long>(), db, ct);
    }

    private async Task ProcessArtistBatchAsync(IDatabase db, List<RedisValue> batch, CancellationToken ct)
    {
        var artistsToSync = new Dictionary<Guid, long>();
        foreach (var idRaw in batch)
        {
            var artistId = Guid.Parse(idRaw.ToString());
            var countRaw = await db.StringGetAsync("artist:" + artistId + ":plays");
            if (countRaw.HasValue && (long)countRaw > 0)
                artistsToSync[artistId] = (long)countRaw;
        }

        if (artistsToSync.Any()) await DispatchSyncAsync(new Dictionary<Guid, long>(), artistsToSync, db, ct);
    }

    private async Task DispatchSyncAsync(Dictionary<Guid, long> tracks, Dictionary<Guid, long> artists, IDatabase db, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new SyncAnalyticsCommand(tracks, artists), ct);
        
        foreach (var kvp in tracks)
        {
            var newVal = await db.StringDecrementAsync("track:" + kvp.Key + ":plays", kvp.Value);
            if (newVal <= 0) await db.SetRemoveAsync("dirty_counters:tracks", kvp.Key.ToString());
        }

        foreach (var kvp in artists)
        {
            var newVal = await db.StringDecrementAsync("artist:" + kvp.Key + ":plays", kvp.Value);
            if (newVal <= 0) await db.SetRemoveAsync("dirty_counters:artists", kvp.Key.ToString());
        }
    }
}

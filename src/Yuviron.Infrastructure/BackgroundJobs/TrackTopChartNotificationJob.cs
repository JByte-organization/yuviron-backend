using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Infrastructure.BackgroundJobs;

public sealed class TrackTopChartNotificationJob : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(24);
    private const string SnapshotKey = "analytics:top-chart:snapshot";
    private const string LastRunKey = "analytics:top-chart:last-run";
    private const string LockKey = "analytics:top-chart:lock";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<TrackTopChartNotificationJob> _logger;

    public TrackTopChartNotificationJob(
        IServiceScopeFactory scopeFactory,
        IConnectionMultiplexer redis,
        TimeProvider timeProvider,
        ILogger<TrackTopChartNotificationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _redis = redis;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        var redisDb = _redis.GetDatabase();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // 1. Try to acquire a distributed lock for 10 minutes to avoid race conditions
        if (!await redisDb.LockTakeAsync(LockKey, Environment.MachineName, TimeSpan.FromMinutes(10)))
        {
            _logger.LogInformation("Top Chart Job is already running on another instance. Skipping.");
            return;
        }

        try
        {
            // 2. Check last run time to avoid running on every restart
            var lastRunRaw = await redisDb.StringGetAsync(LastRunKey);
            if (lastRunRaw.HasValue && DateTime.TryParse(lastRunRaw.ToString(), out var lastRun))
            {
                if (utcNow - lastRun < SyncInterval.Subtract(TimeSpan.FromMinutes(5)))
                {
                    _logger.LogInformation("Top Chart Job already ran at {LastRun}. Skipping startup run.", lastRun);
                    return;
                }
            }

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

            _logger.LogInformation("Starting Top Chart notification processing...");

            var previousSnapshot = await ReadSnapshotAsync(redisDb, cancellationToken);

            var currentTracks = await context.Tracks
                .AsNoTracking()
                .Include(t => t.TrackArtists)
                .Include(t => t.Album)
                    .ThenInclude(a => a!.AlbumArtists)
                .AvailableForPublic(utcNow)
                .OrderByDescending(t => t.PlayCount)
                .ThenBy(t => t.CreatedAt)
                .Take(100)
                .ToListAsync(cancellationToken);

            var currentSnapshot = currentTracks.Select(t => t.Id).ToList();

            if (previousSnapshot is null)
            {
                _logger.LogInformation("No previous snapshot found. Initializing Top Chart state.");
                await WriteSnapshotAsync(redisDb, currentSnapshot, cancellationToken);
                await redisDb.StringSetAsync(LastRunKey, utcNow.ToString("O"));
                return;
            }

            var previousSet = previousSnapshot.ToHashSet();
            var newEntries = currentTracks
                .Select((track, index) => new { Track = track, Rank = index + 1 })
                .Where(x => !previousSet.Contains(x.Track.Id))
                .ToList();

            int eventsSent = 0;
            foreach (var entry in newEntries)
            {
                var artistId = ResolveMainArtistId(entry.Track);
                if (!artistId.HasValue) continue;

                await eventBus.PublishAsync(
                    new TrackEnteredTopChartEvent(
                        artistId.Value,
                        entry.Track.Id,
                        entry.Track.Title,
                        entry.Rank,
                        entry.Track.PlayCount),
                    cancellationToken);
                
                eventsSent++;
            }

            await WriteSnapshotAsync(redisDb, currentSnapshot, cancellationToken);
            await redisDb.StringSetAsync(LastRunKey, utcNow.ToString("O"));
            
            _logger.LogInformation("Top Chart Job completed. Sent {Count} notifications.", eventsSent);
        }
        finally
        {
            await redisDb.LockReleaseAsync(LockKey, Environment.MachineName);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Random jitter on startup to avoid all instances hitting Redis at the exact same millisecond
        await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 10)), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing top chart notifications.");
            }

            try
            {
                // Wait for the next interval
                await Task.Delay(SyncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private static async Task<List<Guid>?> ReadSnapshotAsync(IDatabase redisDb, CancellationToken ct)
    {
        var raw = await redisDb.StringGetAsync(SnapshotKey);
        if (!raw.HasValue) return null;
        return JsonSerializer.Deserialize<List<Guid>>(raw.ToString());
    }

    private static Task WriteSnapshotAsync(IDatabase redisDb, List<Guid> trackIds, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(trackIds);
        return redisDb.StringSetAsync(SnapshotKey, payload);
    }

    private static Guid? ResolveMainArtistId(Track track)
    {
        var mainTrackArtist = track.TrackArtists.FirstOrDefault(ta => ta.Role == ArtistRole.Main)?.ArtistId;
        if (mainTrackArtist.HasValue) return mainTrackArtist;

        var mainAlbumArtist = track.Album?.AlbumArtists.FirstOrDefault(aa => aa.Role == ArtistRole.Main)?.ArtistId;
        if (mainAlbumArtist.HasValue) return mainAlbumArtist;

        return track.Album?.AlbumArtists.FirstOrDefault()?.ArtistId;
    }
}

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
    private static readonly TimeSpan SyncInterval = TimeSpan.FromDays(1);
    private const string SnapshotKey = "analytics:top-chart:snapshot";

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
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var redisDb = _redis.GetDatabase();

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
            await WriteSnapshotAsync(redisDb, currentSnapshot, cancellationToken);
            return;
        }

        var previousSet = previousSnapshot.ToHashSet();
        var newEntries = currentTracks
            .Select((track, index) => new { Track = track, Rank = index + 1 })
            .Where(x => !previousSet.Contains(x.Track.Id))
            .ToList();

        foreach (var entry in newEntries)
        {
            var artistId = ResolveMainArtistId(entry.Track);
            if (!artistId.HasValue)
            {
                continue;
            }

            await eventBus.PublishAsync(
                new TrackEnteredTopChartEvent(
                    artistId.Value,
                    entry.Track.Id,
                    entry.Track.Title,
                    entry.Rank,
                    entry.Track.PlayCount),
                cancellationToken);
        }

        await WriteSnapshotAsync(redisDb, currentSnapshot, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SyncInterval);

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
                await timer.WaitForNextTickAsync(stoppingToken);
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
        if (!raw.HasValue)
        {
            return null;
        }

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
        if (mainTrackArtist.HasValue)
        {
            return mainTrackArtist;
        }

        var mainAlbumArtist = track.Album?.AlbumArtists.FirstOrDefault(aa => aa.Role == ArtistRole.Main)?.ArtistId;
        if (mainAlbumArtist.HasValue)
        {
            return mainAlbumArtist;
        }

        return track.Album?.AlbumArtists.FirstOrDefault()?.ArtistId;
    }
}

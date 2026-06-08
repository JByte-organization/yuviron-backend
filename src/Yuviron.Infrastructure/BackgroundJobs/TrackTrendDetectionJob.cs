using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Yuviron.Application.Abstractions.Analytics;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Infrastructure.BackgroundJobs;

public sealed class TrackTrendDetectionJob : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromDays(1);
    private const string NotifiedKeyPrefix = "analytics:trending:notified:";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<TrackTrendDetectionJob> _logger;

    public TrackTrendDetectionJob(
        IServiceScopeFactory scopeFactory,
        IConnectionMultiplexer redis,
        TimeProvider timeProvider,
        ILogger<TrackTrendDetectionJob> logger)
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
        var analyticsRepository = scope.ServiceProvider.GetRequiredService<IAnalyticsRepository>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var candidates = await analyticsRepository.GetTrendingTracksAsync(
            utcNow.AddDays(-1),
            utcNow.AddDays(-2),
            cancellationToken);

        if (!candidates.Any())
        {
            return;
        }

        var candidateIds = candidates.Select(x => x.TrackId).ToList();
        var tracks = await context.Tracks
            .AsNoTracking()
            .Include(t => t.TrackArtists)
            .Include(t => t.Album)
                .ThenInclude(a => a!.AlbumArtists)
            .Where(t => candidateIds.Contains(t.Id))
            .AvailableForPublic(utcNow)
            .ToListAsync(cancellationToken);

        if (!tracks.Any())
        {
            return;
        }

        var tracksById = tracks.ToDictionary(t => t.Id);
        var notifiedKey = $"{NotifiedKeyPrefix}{utcNow:yyyyMMdd}";
        var redisDb = _redis.GetDatabase();
        var hasNotifiedBefore = await redisDb.KeyExistsAsync(notifiedKey);

        foreach (var candidate in candidates)
        {
            if (!tracksById.TryGetValue(candidate.TrackId, out var track))
            {
                continue;
            }

            var artistId = ResolveMainArtistId(track);
            if (!artistId.HasValue)
            {
                continue;
            }

            var alreadyNotified = hasNotifiedBefore && await redisDb.SetContainsAsync(notifiedKey, candidate.TrackId.ToString());
            if (alreadyNotified)
            {
                continue;
            }

            await eventBus.PublishAsync(
                new TrackTrendingEvent(
                    artistId.Value,
                    track.Id,
                    track.Title,
                    candidate.Current24hPlays,
                    candidate.Previous24hPlays),
                cancellationToken);

            await redisDb.SetAddAsync(notifiedKey, candidate.TrackId.ToString());
        }

        if (await redisDb.KeyExistsAsync(notifiedKey))
        {
            await redisDb.KeyExpireAsync(notifiedKey, TimeSpan.FromDays(3));
        }
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
                _logger.LogError(ex, "Error while processing trending track notifications.");
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

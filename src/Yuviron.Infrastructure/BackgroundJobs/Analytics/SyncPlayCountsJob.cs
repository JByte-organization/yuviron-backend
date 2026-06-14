using Yuviron.Infrastructure.Persistence;
﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

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
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        var tracksToSync = new Dictionary<Guid, long>();
        var artistsToSync = new Dictionary<Guid, long>();

        foreach (var idRaw in trackIdsRaw)
        {
            var trackId = Guid.Parse(idRaw.ToString());
            var countRaw = await db.StringGetAsync($"track:{trackId}:plays"); 
            if (countRaw.HasValue && (long)countRaw > 0)
                tracksToSync[trackId] = (long)countRaw;
        }

        foreach (var idRaw in artistIdsRaw)
        {
            var artistId = Guid.Parse(idRaw.ToString());
            var countRaw = await db.StringGetAsync($"artist:{artistId}:plays");
            if (countRaw.HasValue && (long)countRaw > 0)
                artistsToSync[artistId] = (long)countRaw;
        }

        if (tracksToSync.Any())
        {
            var trackIds = tracksToSync.Keys.ToList();
            var tracks = await context.Tracks
                .Include(t => t.TrackArtists)
                .Include(t => t.Album).ThenInclude(a => a!.AlbumArtists)
                .Where(t => trackIds.Contains(t.Id))
                .ToListAsync(ct);

            var milestoneEvents = new List<TrackPlayMilestoneReachedEvent>();

            foreach (var track in tracks)
            {
                if (!tracksToSync.TryGetValue(track.Id, out var increment) || increment <= 0)
                {
                    continue;
                }

                var oldCount = track.PlayCount;
                track.AddPlays(increment);
                var newCount = track.PlayCount;

                var artistId = track.TrackArtists.FirstOrDefault(ta => ta.Role == ArtistRole.Main)?.ArtistId
                               ?? track.Album!.AlbumArtists.FirstOrDefault(aa => aa.Role == ArtistRole.Main)?.ArtistId
                               ?? track.Album!.AlbumArtists.First().ArtistId;

                foreach (var milestone in new[] { 10_000L, 100_000L, 1_000_000L })
                {
                    if (oldCount < milestone && newCount >= milestone)
                    {
                        milestoneEvents.Add(new TrackPlayMilestoneReachedEvent(
                            artistId,
                            track.Id,
                            track.Title,
                            newCount,
                            milestone));
                    }
                }
            }

            foreach (var milestoneEvent in milestoneEvents)
            {
                await eventBus.PublishAsync(milestoneEvent, ct);
            }
        }

        if (artistsToSync.Any())
        {
            var artistIds = artistsToSync.Keys.ToList();
            var artists = await context.Artists.Where(a => artistIds.Contains(a.Id)).ToListAsync(ct);
            foreach (var artist in artists) artist.AddPlays(artistsToSync[artist.Id]);
        }

        await context.SaveChangesAsync(ct);

        foreach (var kvp in tracksToSync)
        {
            var newVal = await db.StringDecrementAsync($"track:{kvp.Key}:plays", kvp.Value);
            if (newVal == 0) await db.SetRemoveAsync("dirty_counters:tracks", kvp.Key.ToString());
        }

        foreach (var kvp in artistsToSync)
        {
            var newVal = await db.StringDecrementAsync($"artist:{kvp.Key}:plays", kvp.Value);
            if (newVal == 0) await db.SetRemoveAsync("dirty_counters:artists", kvp.Key.ToString());
        }
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Analytics.Commands.SyncAnalytics;

public class SyncAnalyticsHandler : IRequestHandler<SyncAnalyticsCommand>
{
    private readonly ICatalogContext _catalogContext;
    private readonly IEventBus _eventBus;

    public SyncAnalyticsHandler(ICatalogContext catalogContext, IEventBus eventBus)
    {
        _catalogContext = catalogContext;
        _eventBus = eventBus;
    }

    public async Task Handle(SyncAnalyticsCommand request, CancellationToken cancellationToken)
    {
        if (request.TracksToSync.Any())
        {
            var trackIds = request.TracksToSync.Keys.ToList();
            var tracks = await _catalogContext.Tracks
                .Include(t => t.TrackArtists)
                .Include(t => t.Album).ThenInclude(a => a!.AlbumArtists)
                .Where(t => trackIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            var milestoneEvents = new List<TrackPlayMilestoneReachedEvent>();

            foreach (var track in tracks)
            {
                if (!request.TracksToSync.TryGetValue(track.Id, out var increment) || increment <= 0)
                {
                    continue;
                }

                var oldCount = track.PlayCount;
                track.AddPlays(increment);
                var newCount = track.PlayCount;

                var artistId = track.TrackArtists.FirstOrDefault(ta => ta.Role == ArtistRole.Main)?.ArtistId
                               ?? track.Album?.AlbumArtists.FirstOrDefault(aa => aa.Role == ArtistRole.Main)?.ArtistId
                               ?? track.Album?.AlbumArtists.FirstOrDefault()?.ArtistId
                               ?? Guid.Empty;

                if (artistId == Guid.Empty) continue;

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
                await _eventBus.PublishAsync(milestoneEvent, cancellationToken);
            }
        }

        if (request.ArtistsToSync.Any())
        {
            var artistIds = request.ArtistsToSync.Keys.ToList();
            var artists = await _catalogContext.Artists
                .Where(a => artistIds.Contains(a.Id))
                .ToListAsync(cancellationToken);

            foreach (var artist in artists)
            {
                if (request.ArtistsToSync.TryGetValue(artist.Id, out var increment))
                {
                    artist.AddPlays(increment);
                }
            }
        }

        await _catalogContext.SaveChangesAsync(cancellationToken);
    }
}

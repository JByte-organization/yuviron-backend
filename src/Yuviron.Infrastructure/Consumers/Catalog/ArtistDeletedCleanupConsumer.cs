using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Catalog;

public class ArtistDeletedCleanupConsumer : IConsumer<ArtistDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public ArtistDeletedCleanupConsumer(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Consume(ConsumeContext<ArtistDeletedEvent> context)
    {
        var artistId = context.Message.ArtistId;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // Use IgnoreQueryFilters() because we are dealing with a deleted artist 
        // and potentially deleted (but not yet cascade-deleted) relations.
        var affectedAlbums = await _context.Albums
            .IgnoreQueryFilters()
            .Include(a => a.AlbumArtists).ThenInclude(aa => aa.Artist)
            .Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == artistId))
            .ToListAsync(context.CancellationToken);

        foreach (var album in affectedAlbums)
        {
            var activeArtistsCount = album.AlbumArtists.Count(aa => !aa.Artist.IsDeleted || aa.ArtistId == artistId);
            
            if (activeArtistsCount <= 1)
                album.Delete(utcNow);
            else
                album.AlbumArtists.Remove(album.AlbumArtists.First(aa => aa.ArtistId == artistId));
        }

        var affectedTracks = await _context.Tracks
            .IgnoreQueryFilters()
            .Include(t => t.TrackArtists).ThenInclude(ta => ta.Artist)
            .Where(t => t.TrackArtists.Any(ta => ta.ArtistId == artistId))
            .ToListAsync(context.CancellationToken);

        foreach (var track in affectedTracks)
        {
            var activeArtistsCount = track.TrackArtists.Count(ta => !ta.Artist.IsDeleted || ta.ArtistId == artistId);

            if (activeArtistsCount <= 1)
                track.Delete(utcNow);
            else
                track.TrackArtists.Remove(track.TrackArtists.First(ta => ta.ArtistId == artistId));
        }

        await _context.ArtistTeamMembers.IgnoreQueryFilters().Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.ArtistSocialLinks.IgnoreQueryFilters().Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.ArtistPins.IgnoreQueryFilters().Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.UserFollowArtists.IgnoreQueryFilters().Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.ArtistPayoutSettings.IgnoreQueryFilters().Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.ReleaseNotificationTemplates.IgnoreQueryFilters().Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.BannerRequests.IgnoreQueryFilters().Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.VerificationRequests.IgnoreQueryFilters().Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);

        await _context.SaveChangesAsync(context.CancellationToken);
    }
}

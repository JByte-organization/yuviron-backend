using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

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

        var affectedAlbums = await _context.Albums
            .Include(a => a.AlbumArtists)
            .Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == artistId))
            .ToListAsync(context.CancellationToken);

        foreach (var album in affectedAlbums)
        {
            if (album.AlbumArtists.Count == 1)
                album.Delete(utcNow);
            else
                album.AlbumArtists.Remove(album.AlbumArtists.First(aa => aa.ArtistId == artistId));
        }

        var affectedTracks = await _context.Tracks
            .Include(t => t.TrackArtists)
            .Where(t => t.TrackArtists.Any(ta => ta.ArtistId == artistId))
            .ToListAsync(context.CancellationToken);

        foreach (var track in affectedTracks)
        {
            if (track.TrackArtists.Count == 1)
                track.Delete(utcNow);
            else
                track.TrackArtists.Remove(track.TrackArtists.First(ta => ta.ArtistId == artistId));
        }

        await _context.ArtistTeamMembers.Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.ArtistSocialLinks.Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.ArtistPins.Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.UserFollowArtists.Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.ArtistPayoutSettings.Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.ReleaseNotificationTemplates.Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.BannerRequests.Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.VerificationRequests.Where(x => x.ArtistId == artistId).ExecuteDeleteAsync(context.CancellationToken);

        await _context.SaveChangesAsync(context.CancellationToken);
    }
}

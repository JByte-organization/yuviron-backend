using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class HideArtistAlbumsConsumer : IConsumer<ArtistDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public HideArtistAlbumsConsumer(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Consume(ConsumeContext<ArtistDeletedEvent> context)
    {
        var affectedAlbums = await _context.Albums
            .Include(a => a.AlbumArtists)
            .Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == context.Message.ArtistId) && !a.IsDeleted)
            .ToListAsync(context.CancellationToken);

        if (!affectedAlbums.Any()) return;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var album in affectedAlbums)
        {
            if (album.AlbumArtists.Count == 1)
            {
                album.Delete(utcNow);
            }
            else
            {
                var linkToRemove = album.AlbumArtists.First(aa => aa.ArtistId == context.Message.ArtistId);
                album.AlbumArtists.Remove(linkToRemove);
            }
        }

        await _context.SaveChangesAsync(context.CancellationToken);
    }
}
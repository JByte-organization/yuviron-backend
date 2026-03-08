using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Artists.EventHandlers;

public sealed class HideArtistAlbumsEventHandler : INotificationHandler<ArtistDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public HideArtistAlbumsEventHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Handle(ArtistDeletedEvent notification, CancellationToken cancellationToken)
    {
        var affectedAlbums = await _context.Albums
            .Include(a => a.AlbumArtists)
            .Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == notification.ArtistId) && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!affectedAlbums.Any())
        {
            return; 
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var album in affectedAlbums)
        {
            if (album.AlbumArtists.Count == 1)
            {
                album.Delete(utcNow);
            }
            else
            {
                var linkToRemove = album.AlbumArtists.First(aa => aa.ArtistId == notification.ArtistId);
                album.AlbumArtists.Remove(linkToRemove);
                
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
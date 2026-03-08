using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Tracks.EventHandlers;

public sealed class RemoveTrackFromPlaylistsEventHandler : INotificationHandler<TrackDeletedEvent>
{
    private readonly IApplicationDbContext _context;

    public RemoveTrackFromPlaylistsEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(TrackDeletedEvent notification, CancellationToken cancellationToken)
    {
        var playlistLinks = await _context.PlaylistTracks
            .Where(pt => pt.TrackId == notification.TrackId)
            .ToListAsync(cancellationToken);

        if (!playlistLinks.Any())
        {
            return;
        }

        _context.PlaylistTracks.RemoveRange(playlistLinks);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
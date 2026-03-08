using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Albums.EventHandlers;

public sealed class AlbumDeletedEventHandler : INotificationHandler<AlbumDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public AlbumDeletedEventHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Handle(AlbumDeletedEvent notification, CancellationToken cancellationToken)
    {
        var tracks = await _context.Tracks
            .Where(t => t.AlbumId == notification.AlbumId && !t.IsDeleted) 
            .ToListAsync(cancellationToken);

        if (!tracks.Any())
        {
            return;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var track in tracks)
        {
            track.Delete(utcNow); 
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data; // Проверь namespace контекста
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Analytics.EventHandlers;

public sealed class TrackPlayedFallbackEventHandler : INotificationHandler<TrackPlayedFallbackEvent>
{
    private readonly IApplicationDbContext _context;

    public TrackPlayedFallbackEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(TrackPlayedFallbackEvent notification, CancellationToken cancellationToken)
    {
        var track = await _context.Tracks
            .FirstOrDefaultAsync(t => t.Id == notification.TrackId, cancellationToken);
            
        var artist = await _context.Artists
            .FirstOrDefaultAsync(a => a.Id == notification.ArtistId, cancellationToken);

        track?.AddPlays(1);
        artist?.AddPlays(1);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
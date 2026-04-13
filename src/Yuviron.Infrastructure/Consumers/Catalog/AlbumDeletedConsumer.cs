using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class AlbumDeletedConsumer : IConsumer<AlbumDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public AlbumDeletedConsumer(IApplicationDbContext context, TimeProvider timeProvider) 
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Consume(ConsumeContext<AlbumDeletedEvent> context)
    {
        var tracks = await _context.Tracks
            .Where(t => t.AlbumId == context.Message.AlbumId && !t.IsDeleted) 
            .ToListAsync(context.CancellationToken);

        if (!tracks.Any()) return;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var track in tracks)
        {
            track.Delete(utcNow); 
        }
        
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Catalog;

public class AlbumDeletedCleanupConsumer : IConsumer<AlbumDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public AlbumDeletedCleanupConsumer(IApplicationDbContext context, TimeProvider timeProvider) 
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Consume(ConsumeContext<AlbumDeletedEvent> context)
    {
        var albumId = context.Message.AlbumId;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var tracks = await _context.Tracks
            .Where(t => t.AlbumId == albumId) 
            .ToListAsync(context.CancellationToken);

        foreach (var track in tracks)
        {
            track.Delete(utcNow); 
        }

        await _context.UserSavedAlbums.Where(ua => ua.AlbumId == albumId).ExecuteDeleteAsync(context.CancellationToken);
        
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}

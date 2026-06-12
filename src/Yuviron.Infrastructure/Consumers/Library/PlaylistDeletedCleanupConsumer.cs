using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Library;

public class PlaylistDeletedCleanupConsumer : IConsumer<PlaylistDeletedEvent>
{
    private readonly IApplicationDbContext _context;

    public PlaylistDeletedCleanupConsumer(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<PlaylistDeletedEvent> context)
    {
        var playlistId = context.Message.PlaylistId;

        await _context.PlaylistTracks.IgnoreQueryFilters().Where(pt => pt.PlaylistId == playlistId).ExecuteDeleteAsync(context.CancellationToken);
        
        await _context.UserSavedPlaylists.IgnoreQueryFilters().Where(usp => usp.PlaylistId == playlistId).ExecuteDeleteAsync(context.CancellationToken);
    }
}

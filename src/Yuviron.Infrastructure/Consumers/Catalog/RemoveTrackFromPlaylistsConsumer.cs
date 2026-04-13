using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class RemoveTrackFromPlaylistsConsumer : IConsumer<TrackDeletedEvent>
{
    private readonly IApplicationDbContext _context;

    public RemoveTrackFromPlaylistsConsumer(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<TrackDeletedEvent> context)
    {
        var playlistLinks = await _context.PlaylistTracks
            .Where(pt => pt.TrackId == context.Message.TrackId)
            .ToListAsync(context.CancellationToken);

        if (!playlistLinks.Any()) return;

        _context.PlaylistTracks.RemoveRange(playlistLinks);
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}
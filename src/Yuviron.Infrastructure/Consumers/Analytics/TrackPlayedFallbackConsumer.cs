using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class TrackPlayedFallbackConsumer : IConsumer<TrackPlayedFallbackEvent>
{
    private readonly IApplicationDbContext _context;

    public TrackPlayedFallbackConsumer(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<TrackPlayedFallbackEvent> context)
    {
        var track = await _context.Tracks
            .FirstOrDefaultAsync(t => t.Id == context.Message.TrackId, context.CancellationToken);
            
        var artist = await _context.Artists
            .FirstOrDefaultAsync(a => a.Id == context.Message.ArtistId, context.CancellationToken);

        track?.AddPlays(1);
        artist?.AddPlays(1);

        await _context.SaveChangesAsync(context.CancellationToken);
    }
}
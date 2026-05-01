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
            .Include(t => t.TrackArtists)
            .ThenInclude(ta => ta.Artist) 
            .FirstOrDefaultAsync(t => t.Id == context.Message.TrackId, context.CancellationToken);
            
        if (track != null)
        {
            track.AddPlays(1);

            foreach (var trackArtist in track.TrackArtists)
            {
                if (trackArtist.Artist != null)
                {
                    trackArtist.Artist.AddPlays(1);
                }
            }

            await _context.SaveChangesAsync(context.CancellationToken);
        }
    }
}
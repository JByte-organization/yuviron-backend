using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class TrackPlayedFallbackConsumer : IConsumer<TrackPlayedFallbackEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public TrackPlayedFallbackConsumer(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Consume(ConsumeContext<TrackPlayedFallbackEvent> context)
    {
        var trackId = context.Message.TrackId;
        var userId = context.Message.UserId;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var lastPlayTime = await _context.ListeningEvents
            .Where(le => le.UserId == userId)
            .OrderByDescending(le => le.PlayedAt)
            .Select(le => le.PlayedAt)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (lastPlayTime != default && (utcNow - lastPlayTime).TotalSeconds < 30)
        {
            return; 
        }

        var listeningEvent = ListeningEvent.Create(
            userId: userId,
            trackId: trackId,
            msPlayed: 30000,
            deviceType: PlaybackDeviceType.Unknown,
            countryCode: null,
            sourceType: PlaybackSourceType.Unknown,
            sourceId: null,
            utcNow: utcNow
        );
        
        _context.ListeningEvents.Add(listeningEvent);

        var track = await _context.Tracks
            .Include(t => t.TrackArtists)
            .ThenInclude(ta => ta.Artist) 
            .FirstOrDefaultAsync(t => t.Id == trackId, context.CancellationToken);
            
        if (track != null)
        {
            track.AddPlays(1);

            foreach (var trackArtist in track.TrackArtists)
            {
                trackArtist.Artist?.AddPlays(1);
            }
        }

        await _context.SaveChangesAsync(context.CancellationToken);
    }
}
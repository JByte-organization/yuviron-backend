using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class TrackSuccessfullyPlayedConsumer : IConsumer<TrackSuccessfullyPlayedEvent>
{
    private readonly IApplicationDbContext _context;

    public TrackSuccessfullyPlayedConsumer(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<TrackSuccessfullyPlayedEvent> context)
    {
        var msg = context.Message;

        var listeningEvent = ListeningEvent.Create(
            msg.UserId,
            msg.TrackId,
            msg.MsPlayed,
            msg.DeviceType,
            null,
            msg.SourceType,
            msg.SourceId,
            msg.PlayedAt
        );

        _context.ListeningEvents.Add(listeningEvent);
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}
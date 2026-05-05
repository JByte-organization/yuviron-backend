using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Tracks.Consumers;

public sealed class TrackDeletedConsumer : IConsumer<TrackDeletedEvent>
{
    private readonly IApplicationDbContext _context;

    public TrackDeletedConsumer(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<TrackDeletedEvent> context)
    {
        var trackId = context.Message.TrackId;

        var deadMappings = await _context.ExternalMappings
            .Where(m => m.InternalId == trackId && m.EntityType == nameof(Track))
            .ToListAsync(context.CancellationToken);

        if (deadMappings.Any())
        {
            _context.ExternalMappings.RemoveRange(deadMappings);
            await _context.SaveChangesAsync(context.CancellationToken);
        }
    }
}
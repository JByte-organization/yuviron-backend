using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Consumers.Catalog;

public sealed class TrackDeletedCleanupConsumer : IConsumer<TrackDeletedEvent>
{
    private readonly IApplicationDbContext _context;

    public TrackDeletedCleanupConsumer(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<TrackDeletedEvent> context)
    {
        var trackId = context.Message.TrackId;

        await _context.PlaylistTracks.Where(pt => pt.TrackId == trackId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.UserSavedTracks.Where(ut => ut.TrackId == trackId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.TrackGenres.Where(tg => tg.TrackId == trackId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.TrackMoods.Where(tm => tm.TrackId == trackId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.ExternalMappings.Where(m => m.InternalId == trackId && m.EntityType == nameof(Track)).ExecuteDeleteAsync(context.CancellationToken);
    }
}

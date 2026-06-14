using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
﻿using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Consumers.Catalog;

public sealed class TrackDeletedCleanupConsumer : IConsumer<TrackDeletedEvent>
{
    private readonly AppDbContext _context;

    public TrackDeletedCleanupConsumer(AppDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<TrackDeletedEvent> context)
    {
        var trackId = context.Message.TrackId;

        await _context.PlaylistTracks.IgnoreQueryFilters().Where(pt => pt.TrackId == trackId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.UserSavedTracks.IgnoreQueryFilters().Where(ut => ut.TrackId == trackId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.TrackGenres.IgnoreQueryFilters().Where(tg => tg.TrackId == trackId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.TrackMoods.IgnoreQueryFilters().Where(tm => tm.TrackId == trackId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.ExternalMappings.IgnoreQueryFilters().Where(m => m.InternalId == trackId && m.EntityType == nameof(Track)).ExecuteDeleteAsync(context.CancellationToken);
    }
}

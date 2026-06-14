using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Common;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public abstract class NotifyArtistTeamConsumerBase<TEvent> : IConsumer<TEvent>
    where TEvent : class, IArtistEvent
{
    protected readonly AppDbContext Context;
    protected readonly INotificationService NotificationService;

    protected NotifyArtistTeamConsumerBase(AppDbContext context, INotificationService notificationService)
    {
        Context = context;
        NotificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var msg = context.Message;
        
        var teamIds = await Context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.ArtistId == msg.ArtistId)
            .Select(tm => tm.UserId)
            .ToListAsync(context.CancellationToken);

        if (!teamIds.Any()) return;

        await SendNotificationAsync(msg, teamIds, context.CancellationToken);
    }

    protected abstract Task SendNotificationAsync(TEvent msg, List<Guid> teamIds, CancellationToken ct);
}
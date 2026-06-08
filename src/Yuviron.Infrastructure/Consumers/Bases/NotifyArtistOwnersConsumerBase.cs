using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Infrastructure.Consumers.Bases;

public abstract class NotifyArtistOwnersConsumerBase<TEvent> : IConsumer<TEvent>
    where TEvent : class, IArtistEvent
{
    protected readonly IApplicationDbContext Context;
    protected readonly INotificationService NotificationService;

    protected NotifyArtistOwnersConsumerBase(IApplicationDbContext context, INotificationService notificationService)
    {
        Context = context;
        NotificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var msg = context.Message;
        
        var ownerIds = await Context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.ArtistId == msg.ArtistId && tm.Role == ArtistTeamRole.Owner)
            .Select(tm => tm.UserId)
            .ToListAsync(context.CancellationToken);

        if (!ownerIds.Any()) return;

        await SendNotificationAsync(msg, ownerIds, context.CancellationToken);
        
        // Вызываем хук для наследников
        await AfterNotificationSentAsync(msg, ownerIds, context.CancellationToken);
    }

    protected abstract Task SendNotificationAsync(TEvent msg, List<Guid> ownerIds, CancellationToken ct);

    // Хук по умолчанию пуст
    protected virtual Task AfterNotificationSentAsync(TEvent msg, List<Guid> ownerIds, CancellationToken ct) 
        => Task.CompletedTask;
}
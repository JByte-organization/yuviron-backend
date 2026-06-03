using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class NotifyOwnersOnPayoutRejectedConsumer : IConsumer<PayoutRejectedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyOwnersOnPayoutRejectedConsumer(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<PayoutRejectedEvent> context)
    {
        var msg = context.Message;
        
        var ownerIds = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.ArtistId == msg.ArtistId && tm.Role == ArtistTeamRole.Owner)
            .Select(tm => tm.UserId)
            .ToListAsync(context.CancellationToken);

        if (!ownerIds.Any()) return;

        await _notificationService.SendToUsersAsync(
            userIds: ownerIds,
            category: NotificationCategory.Billing,
            type: "payout_rejected",
            title: "Увага: Заявку на виплату відхилено ❌",
            body: $"Ваш запит на виведення ${msg.Amount:F2} відхилено адміністратором. Причина: {msg.Reason}. Гроші повернено на ваш баланс.",
            entityType: NotificationEntityType.Artist,
            entityId: msg.ArtistId,
            cancellationToken: context.CancellationToken
        );
    }
}
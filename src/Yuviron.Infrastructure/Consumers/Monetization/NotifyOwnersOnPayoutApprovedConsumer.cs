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

public class NotifyOwnersOnPayoutApprovedConsumer : IConsumer<PayoutApprovedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyOwnersOnPayoutApprovedConsumer(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<PayoutApprovedEvent> context)
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
            type: "payout_approved",
            title: "Заявка на виплату схвалена! 💸",
            body: $"Ваш запит на виведення ${msg.Amount:F2} успішно оброблено. Гроші вже в дорозі!",
            entityType: NotificationEntityType.Artist,
            entityId: msg.ArtistId,
            cancellationToken: context.CancellationToken
        );
    }
}
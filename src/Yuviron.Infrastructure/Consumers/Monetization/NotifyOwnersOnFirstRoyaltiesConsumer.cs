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

public class NotifyOwnersOnFirstRoyaltiesConsumer : IConsumer<FirstRoyaltiesEarnedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyOwnersOnFirstRoyaltiesConsumer(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<FirstRoyaltiesEarnedEvent> context)
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
            type: "first_royalties",
            title: "Перші гроші! 🎉",
            body: "Вітаємо! Ваш трек послухали достатньо разів, щоб ви отримали свої перші роялті. Загляніть у гаманець!",
            entityType: NotificationEntityType.Artist,
            entityId: msg.ArtistId,
            cancellationToken: context.CancellationToken
        );
    }
}
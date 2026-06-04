using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers.Bases;

namespace Yuviron.Infrastructure.Consumers;

public class NotifyOwnersOnFirstRoyaltiesConsumer : NotifyArtistOwnersConsumerBase<FirstRoyaltiesEarnedEvent>
{
    public NotifyOwnersOnFirstRoyaltiesConsumer(IApplicationDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override async Task SendNotificationAsync(FirstRoyaltiesEarnedEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        await NotificationService.SendToUsersAsync(ownerIds, NotificationCategory.Billing, "first_royalties", 
            "Перші гроші! 🎉", "Вітаємо! Ваш трек послухали достатньо разів, щоб ви отримали свої перші роялті.", NotificationEntityType.Artist, msg.ArtistId, ct);
}
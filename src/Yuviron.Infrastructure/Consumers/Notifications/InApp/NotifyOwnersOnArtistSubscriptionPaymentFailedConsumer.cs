using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyOwnersOnArtistSubscriptionPaymentFailedConsumer : NotifyArtistOwnersConsumerBase<ArtistSubscriptionPaymentFailedEvent>
{
    public NotifyOwnersOnArtistSubscriptionPaymentFailedConsumer(AppDbContext context, INotificationService notificationService)
        : base(context, notificationService)
    {
    }

    protected override Task SendNotificationAsync(ArtistSubscriptionPaymentFailedEvent msg, List<Guid> ownerIds, CancellationToken ct)
    {
        var body = $"Не вдалося списати платіж за Artist Pro для профілю. Підписка «{msg.PlanName}» переведена в статус проблемної.";

        if (!string.IsNullOrWhiteSpace(msg.FailureReason))
        {
            body += $" Деталі: {msg.FailureReason}";
        }

        return NotificationService.SendToUsersAsync(
            ownerIds,
            NotificationCategory.Billing,
            "artist_subscription_payment_failed",
            "Проблема з оплатою Artist Pro",
            body,
            NotificationEntityType.Artist,
            msg.ArtistId,
            ct);
    }
}

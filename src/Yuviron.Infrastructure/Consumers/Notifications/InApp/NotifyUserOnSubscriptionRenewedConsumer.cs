using MassTransit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyUserOnSubscriptionRenewedConsumer : IConsumer<SubscriptionRenewedEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUserOnSubscriptionRenewedConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<SubscriptionRenewedEvent> context) =>
        _notificationService.SendToUserAsync(
            context.Message.UserId,
            NotificationCategory.Billing,
            "subscription_renewed",
            "Підписку продовжено",
            $"Успішно списано оплату за «{context.Message.PlanName}». Доступ продовжено до {context.Message.EndAt:yyyy-MM-dd HH:mm} UTC.",
            NotificationEntityType.System,
            null,
            context.CancellationToken);
}

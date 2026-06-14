using MassTransit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyUserOnSubscriptionPaymentFailedConsumer : IConsumer<SubscriptionPaymentFailedEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUserOnSubscriptionPaymentFailedConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<SubscriptionPaymentFailedEvent> context)
    {
        var body = $"Не вдалося списати платіж за план «{context.Message.PlanName}». Перевірте картку та повторіть спробу.";

        if (!string.IsNullOrWhiteSpace(context.Message.FailureReason))
        {
            body += $" Деталі: {context.Message.FailureReason}";
        }

        return _notificationService.SendToUserAsync(
            context.Message.UserId,
            NotificationCategory.Billing,
            "subscription_payment_failed",
            "Проблема з оплатою Premium",
            body,
            NotificationEntityType.System,
            null,
            context.CancellationToken);
    }
}

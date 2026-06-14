using MassTransit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyUserOnSubscriptionCanceledConsumer : IConsumer<SubscriptionCanceledEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUserOnSubscriptionCanceledConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<SubscriptionCanceledEvent> context) =>
        _notificationService.SendToUserAsync(
            context.Message.UserId,
            NotificationCategory.Billing,
            "subscription_canceled",
            "Автопродовження скасовано",
            $"Автопродовження підписки «{context.Message.PlanName}» було успішно вимкнено.",
            NotificationEntityType.System,
            null,
            context.CancellationToken);
}

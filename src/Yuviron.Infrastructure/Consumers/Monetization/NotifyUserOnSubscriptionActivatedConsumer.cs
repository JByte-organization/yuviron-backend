using MassTransit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public sealed class NotifyUserOnSubscriptionActivatedConsumer : IConsumer<SubscriptionActivatedEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUserOnSubscriptionActivatedConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<SubscriptionActivatedEvent> context) =>
        _notificationService.SendToUserAsync(
            context.Message.UserId,
            NotificationCategory.Billing,
            "subscription_activated",
            "Premium активовано",
            $"Ваша підписка «{context.Message.PlanName}» активована до {context.Message.EndAt:yyyy-MM-dd HH:mm} UTC.",
            NotificationEntityType.System,
            null,
            context.CancellationToken);
}

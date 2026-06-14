using MassTransit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyUserOnBannerRequestPaidConsumer : IConsumer<BannerRequestPaidEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUserOnBannerRequestPaidConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<BannerRequestPaidEvent> context) =>
        _notificationService.SendToUserAsync(
            context.Message.UserId,
            NotificationCategory.Billing,
            "banner_payment_fulfilled",
            "Оплату банера підтверджено",
            $"Оплату банера «{context.Message.BannerTitle}» успішно підтверджено.",
            NotificationEntityType.Artist,
            context.Message.ArtistId,
            context.CancellationToken);
}

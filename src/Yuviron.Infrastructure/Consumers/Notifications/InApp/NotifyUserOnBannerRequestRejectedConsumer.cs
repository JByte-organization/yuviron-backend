using MassTransit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyUserOnBannerRequestRejectedConsumer : IConsumer<BannerRequestRejectedEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUserOnBannerRequestRejectedConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<BannerRequestRejectedEvent> context)
    {
        var reason = string.IsNullOrWhiteSpace(context.Message.Reason) ? "без додаткових пояснень" : context.Message.Reason;

        return _notificationService.SendToUserAsync(
            context.Message.UserId,
            NotificationCategory.Billing,
            "banner_request_rejected",
            "Банер відхилено",
            $"Ваш банер «{context.Message.BannerTitle}» не пройшов модерацію. Причина: {reason}",
            NotificationEntityType.Artist,
            context.Message.ArtistId,
            context.CancellationToken);
    }
}

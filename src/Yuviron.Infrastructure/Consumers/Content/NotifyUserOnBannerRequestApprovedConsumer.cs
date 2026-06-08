using MassTransit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public sealed class NotifyUserOnBannerRequestApprovedConsumer : IConsumer<BannerRequestApprovedEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUserOnBannerRequestApprovedConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<BannerRequestApprovedEvent> context) =>
        _notificationService.SendToUserAsync(
            context.Message.UserId,
            NotificationCategory.Billing,
            "banner_request_approved",
            "Банер схвалено",
            $"Ваш банер «{context.Message.BannerTitle}» пройшов модерацію та був схвалений.",
            NotificationEntityType.Artist,
            context.Message.ArtistId,
            context.CancellationToken);
}

using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyOwnersOnBannerStartedConsumer : NotifyArtistOwnersConsumerBase<BannerStartedEvent>
{
    public NotifyOwnersOnBannerStartedConsumer(AppDbContext context, INotificationService notificationService)
        : base(context, notificationService)
    {
    }

    protected override Task SendNotificationAsync(BannerStartedEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        NotificationService.SendToUsersAsync(
            ownerIds,
            NotificationCategory.Billing,
            "banner_started",
            "Початок показу банера",
            $"Ваш банер «{msg.BannerTitle}» почав показ на головній сторінці Yuviron.",
            NotificationEntityType.Artist,
            msg.ArtistId,
            ct);
}

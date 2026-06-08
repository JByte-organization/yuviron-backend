using MassTransit;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers.Bases;

namespace Yuviron.Infrastructure.Consumers.Content;

public sealed class NotifyOwnersOnBannerStartedConsumer : NotifyArtistOwnersConsumerBase<BannerStartedEvent>
{
    public NotifyOwnersOnBannerStartedConsumer(IApplicationDbContext context, INotificationService notificationService)
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

using MassTransit;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers.Bases;

namespace Yuviron.Infrastructure.Consumers.Analytics;

public sealed class NotifyOwnersOnTrackEnteredTopChartConsumer : NotifyArtistOwnersConsumerBase<TrackEnteredTopChartEvent>
{
    public NotifyOwnersOnTrackEnteredTopChartConsumer(IApplicationDbContext context, INotificationService notificationService)
        : base(context, notificationService)
    {
    }

    protected override Task SendNotificationAsync(TrackEnteredTopChartEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        NotificationService.SendToUsersAsync(
            ownerIds,
            NotificationCategory.Music,
            "track_entered_top_chart",
            "Трек у топ-чарті",
            $"Трек «{msg.TrackTitle}» увійшов у глобальний топ-чарт Yuviron і зараз на {msg.Rank}-му місці з {msg.PlayCount:N0} прослуховуваннями.",
            NotificationEntityType.Track,
            msg.TrackId,
            ct);
}

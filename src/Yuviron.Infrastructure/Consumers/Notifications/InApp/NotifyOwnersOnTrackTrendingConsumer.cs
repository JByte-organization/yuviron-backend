using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyOwnersOnTrackTrendingConsumer : NotifyArtistOwnersConsumerBase<TrackTrendingEvent>
{
    public NotifyOwnersOnTrackTrendingConsumer(AppDbContext context, INotificationService notificationService)
        : base(context, notificationService)
    {
    }

    protected override Task SendNotificationAsync(TrackTrendingEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        NotificationService.SendToUsersAsync(
            ownerIds,
            NotificationCategory.Music,
            "track_trending",
            "Трек у тренді",
            $"Трек «{msg.TrackTitle}» різко виріс за прослуховуваннями: {msg.Current24hPlays:N0} за останні 24 години проти {msg.Previous24hPlays:N0} у попередній добі.",
            NotificationEntityType.Track,
            msg.TrackId,
            ct);
}

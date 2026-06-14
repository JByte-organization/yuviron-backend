using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyOwnersOnArtistSubscriptionActivatedConsumer : NotifyArtistOwnersConsumerBase<ArtistSubscriptionActivatedEvent>
{
    public NotifyOwnersOnArtistSubscriptionActivatedConsumer(AppDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override Task SendNotificationAsync(ArtistSubscriptionActivatedEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        NotificationService.SendToUsersAsync(
            ownerIds,
            NotificationCategory.Billing,
            "artist_subscription_activated",
            "Artist Pro активовано",
            $"Підписку Artist Pro для профілю активовано до {msg.EndAt:yyyy-MM-dd HH:mm} UTC.",
            NotificationEntityType.Artist,
            msg.ArtistId,
            ct);
}

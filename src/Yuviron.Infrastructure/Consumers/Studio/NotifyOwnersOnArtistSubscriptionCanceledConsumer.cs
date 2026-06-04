using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers.Bases;

namespace Yuviron.Infrastructure.Consumers;

public class NotifyOwnersOnArtistSubscriptionCanceledConsumer : NotifyArtistOwnersConsumerBase<ArtistSubscriptionCanceledEvent>
{
    public NotifyOwnersOnArtistSubscriptionCanceledConsumer(IApplicationDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override async Task SendNotificationAsync(ArtistSubscriptionCanceledEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        await NotificationService.SendToUsersAsync(ownerIds, NotificationCategory.Billing, "subscription_canceled", 
            "Підписку скасовано 😢", "Автоподовження підписки Artist Pro скасовано. Ви збережете доступ до кінця періоду.", NotificationEntityType.Artist, msg.ArtistId, ct);
}
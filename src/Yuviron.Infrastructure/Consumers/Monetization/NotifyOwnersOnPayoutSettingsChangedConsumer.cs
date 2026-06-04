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

public class NotifyOwnersOnPayoutSettingsChangedConsumer : NotifyArtistOwnersConsumerBase<PayoutSettingsChangedEvent>
{
    public NotifyOwnersOnPayoutSettingsChangedConsumer(IApplicationDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override async Task SendNotificationAsync(PayoutSettingsChangedEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        await NotificationService.SendToUsersAsync(ownerIds, NotificationCategory.System, "payout_settings_changed", 
            "Реквізити для виплат змінено ⚠️", "Платіжні реквізити вашого профілю були оновлені.", NotificationEntityType.Artist, msg.ArtistId, ct);
}
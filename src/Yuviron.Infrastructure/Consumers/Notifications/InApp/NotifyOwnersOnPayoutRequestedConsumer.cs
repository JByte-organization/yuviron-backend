using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public class NotifyOwnersOnPayoutRequestedConsumer : NotifyArtistOwnersConsumerBase<PayoutRequestedEvent>
{
    public NotifyOwnersOnPayoutRequestedConsumer(AppDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override async Task SendNotificationAsync(PayoutRequestedEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        await NotificationService.SendToUsersAsync(ownerIds, NotificationCategory.Billing, "payout_requested", 
            "Запит на виплату прийнято ⏳", $"Ваш запит на виведення ${msg.Amount:F2} успішно надіслано. Ми повідомимо вас після перевірки.", NotificationEntityType.Artist, msg.ArtistId, ct);
}
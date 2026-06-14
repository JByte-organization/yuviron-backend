using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public class NotifyOwnersOnPayoutRejectedConsumer : NotifyArtistOwnersConsumerBase<PayoutRejectedEvent>
{
    public NotifyOwnersOnPayoutRejectedConsumer(AppDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override async Task SendNotificationAsync(PayoutRejectedEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        await NotificationService.SendToUsersAsync(ownerIds, NotificationCategory.Billing, "payout_rejected", 
            "Увага: Заявку на виплату відхилено ❌", $"Ваш запит на виведення ${msg.Amount:F2} відхилено адміністратором. Причина: {msg.Reason}", NotificationEntityType.Artist, msg.ArtistId, ct);
}
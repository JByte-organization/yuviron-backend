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

public class NotifyOwnersOnPayoutApprovedConsumer : NotifyArtistOwnersConsumerBase<PayoutApprovedEvent>
{
    public NotifyOwnersOnPayoutApprovedConsumer(AppDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override async Task SendNotificationAsync(PayoutApprovedEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        await NotificationService.SendToUsersAsync(ownerIds, NotificationCategory.Billing, "payout_approved", 
            "Заявка на виплату схвалена! 💸", $"Ваш запит на виведення ${msg.Amount:F2} успішно оброблено. Гроші вже в дорозі!", NotificationEntityType.Artist, msg.ArtistId, ct);
}
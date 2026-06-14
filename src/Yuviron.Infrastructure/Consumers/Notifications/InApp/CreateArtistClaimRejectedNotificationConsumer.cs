using MassTransit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public class CreateArtistClaimRejectedNotificationConsumer : IConsumer<ArtistClaimRejectedEvent>
{
    private readonly INotificationService _notificationService;
    
    private const string NotificationType = "artist_claim_rejected";

    public CreateArtistClaimRejectedNotificationConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<ArtistClaimRejectedEvent> context)
    {
        var msg = context.Message;
        var reasonText = string.IsNullOrWhiteSpace(msg.AdminNote) ? "недостатньо доказів." : msg.AdminNote;

        await _notificationService.SendToUserAsync(
            userId: msg.UserId,
            category: NotificationCategory.System, 
            type: NotificationType, 
            title: "Заявку на профіль відхилено ❌",
            body: $"Ваша заявка на профіль «{msg.ArtistName}» була відхилена. Причина: {reasonText}",
            entityType: NotificationEntityType.Artist,
            entityId: msg.ArtistId,
            cancellationToken: context.CancellationToken
        );
    }
}
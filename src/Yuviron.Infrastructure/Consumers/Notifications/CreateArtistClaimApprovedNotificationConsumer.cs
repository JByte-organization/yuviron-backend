using MassTransit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class CreateArtistClaimApprovedNotificationConsumer : IConsumer<ArtistClaimApprovedEvent>
{
    private readonly INotificationService _notificationService;

    public CreateArtistClaimApprovedNotificationConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<ArtistClaimApprovedEvent> context)
    {
        var msg = context.Message;
        
        await _notificationService.SendToUserAsync(
            userId: msg.UserId,
            title: "Заявка на профіль схвалена! 🎵",
            body: $"Вітаємо! Ваш профіль артиста «{msg.ArtistName}» успішно підтверджено. Тепер ви маєте доступ до Студії.",
            entityType: NotificationEntityType.Artist,
            entityId: msg.ArtistId,
            cancellationToken: context.CancellationToken
        );
    }
}
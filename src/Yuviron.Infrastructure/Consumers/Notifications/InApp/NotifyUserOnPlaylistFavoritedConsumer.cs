using MassTransit;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public class NotifyUserOnPlaylistFavoritedConsumer : IConsumer<PlaylistAddedToFavoritesEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUserOnPlaylistFavoritedConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<PlaylistAddedToFavoritesEvent> context)
    {
        var msg = context.Message;

        // Не відправляємо власнику плейлиста, якщо він сам зберіг свій плейлист
        if (msg.SavedByUserId == msg.PlaylistOwnerId) return;

        await _notificationService.SendToUserAsync(
            msg.PlaylistOwnerId, 
            NotificationCategory.Social, 
            "playlist_favorited", 
            "Ваш плейлист оцінили! 🎵", 
            $"Користувач {msg.SavedByUserName} додав ваш плейлист «{msg.PlaylistName}» у свою медіатеку.", 
            NotificationEntityType.Playlist, 
            msg.PlaylistId, 
            context.CancellationToken);
    }
}

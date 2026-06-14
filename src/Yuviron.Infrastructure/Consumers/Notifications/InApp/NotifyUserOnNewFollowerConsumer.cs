using MassTransit;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public class NotifyUserOnNewFollowerConsumer : IConsumer<UserFollowedUserEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUserOnNewFollowerConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<UserFollowedUserEvent> context)
    {
        var msg = context.Message;

        // Не відправляємо самому собі
        if (msg.FollowerId == msg.TargetUserId) return;

        await _notificationService.SendToUserAsync(
            msg.TargetUserId, 
            NotificationCategory.Social, 
            "new_follower", 
            "Новий підписник! 🫂", 
            $"Користувач {msg.FollowerName} підписався на вас. Подивіться, що він слухає!", 
            NotificationEntityType.User, 
            msg.FollowerId, 
            context.CancellationToken);
    }
}

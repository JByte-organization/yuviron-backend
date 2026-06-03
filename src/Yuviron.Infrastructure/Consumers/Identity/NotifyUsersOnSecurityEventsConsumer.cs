using MassTransit;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class NotifyUsersOnSecurityEventsConsumer : IConsumer<NewDeviceLoginEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUsersOnSecurityEventsConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<NewDeviceLoginEvent> context)
    {
        var msg = context.Message;

        await _notificationService.SendToUserAsync(
            userId: msg.UserId,
            category: NotificationCategory.System,
            type: "new_device_login",
            title: "Новий вхід в акаунт 🛡️",
            body: $"Був виконаний вхід з пристрою {msg.DeviceName} ({msg.BrowserName}). IP: {msg.IpAddress}. Якщо це були не ви, терміново змініть пароль!",
            entityType: NotificationEntityType.System, 
            entityId: null,
            cancellationToken: context.CancellationToken
        );
    }
}
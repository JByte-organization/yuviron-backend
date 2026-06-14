using MassTransit;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public class NotifyUsersOnSecurityEventsConsumer : IConsumer<NewDeviceLoginEvent>
{
    private readonly INotificationService _notificationService;
    public NotifyUsersOnSecurityEventsConsumer(INotificationService notificationService) => _notificationService = notificationService;

    public async Task Consume(ConsumeContext<NewDeviceLoginEvent> context) =>
        await _notificationService.SendToUserAsync(context.Message.UserId, NotificationCategory.System, "new_device_login", 
            "Новий вхід в акаунт 🛡️", $"Вхід з пристрою {context.Message.DeviceName} ({context.Message.BrowserName}). IP: {context.Message.IpAddress}.", NotificationEntityType.System, null, context.CancellationToken);
}
using MassTransit;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public class NotifyUserOnTeamRoleChangedConsumer : IConsumer<TeamRoleChangedEvent>
{
    private readonly INotificationService _notificationService;
    public NotifyUserOnTeamRoleChangedConsumer(INotificationService notificationService) => _notificationService = notificationService;

    public async Task Consume(ConsumeContext<TeamRoleChangedEvent> context) =>
        await _notificationService.SendToUserAsync(context.Message.TargetUserId, NotificationCategory.System, "team_role_changed", 
            "Зміна прав доступу 🔑", $"Ваша роль в команді {context.Message.ArtistName} змінена на «{context.Message.NewRole}».", NotificationEntityType.Artist, context.Message.ArtistId, context.CancellationToken);
}
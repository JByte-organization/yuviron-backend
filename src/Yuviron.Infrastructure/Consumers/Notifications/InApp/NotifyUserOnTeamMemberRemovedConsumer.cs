using MassTransit;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public class NotifyUserOnTeamMemberRemovedConsumer : IConsumer<TeamMemberRemovedEvent>
{
    private readonly INotificationService _notificationService;
    public NotifyUserOnTeamMemberRemovedConsumer(INotificationService notificationService) => _notificationService = notificationService;

    public async Task Consume(ConsumeContext<TeamMemberRemovedEvent> context) =>
        await _notificationService.SendToUserAsync(context.Message.UserId, NotificationCategory.System, "team_member_removed", 
            "Доступ відкликано 🔒", $"Ваш доступ до профілю «{context.Message.ArtistName}» був відкликаний.", NotificationEntityType.Artist, context.Message.ArtistId, context.CancellationToken);
}
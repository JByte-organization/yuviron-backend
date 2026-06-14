using MassTransit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyUserOnComplaintRejectedConsumer : IConsumer<ComplaintRejectedEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUserOnComplaintRejectedConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<ComplaintRejectedEvent> context)
    {
        var body = $"Вашу скаргу на {GetTargetLabel(context.Message.TargetType)} «{context.Message.TargetTitle}» розглянуто, але не підтверджено.";

        if (!string.IsNullOrWhiteSpace(context.Message.ModerationNote))
        {
            body += $" Причина: {context.Message.ModerationNote}";
        }

        return _notificationService.SendToUserAsync(
            context.Message.UserId,
            NotificationCategory.System,
            "complaint_rejected",
            "Скаргу відхилено",
            body,
            GetEntityType(context.Message.TargetType),
            context.Message.TargetId,
            context.CancellationToken);
    }

    private static NotificationEntityType GetEntityType(ComplaintTargetType targetType) =>
        targetType switch
        {
            ComplaintTargetType.Track => NotificationEntityType.Track,
            ComplaintTargetType.Album => NotificationEntityType.Album,
            ComplaintTargetType.Artist => NotificationEntityType.Artist,
            _ => NotificationEntityType.System
        };

    private static string GetTargetLabel(ComplaintTargetType targetType) =>
        targetType switch
        {
            ComplaintTargetType.Track => "трек",
            ComplaintTargetType.Album => "альбом",
            ComplaintTargetType.Artist => "артиста",
            _ => "контент"
        };
}

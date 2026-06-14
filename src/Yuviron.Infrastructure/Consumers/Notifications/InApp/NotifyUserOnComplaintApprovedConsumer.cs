using MassTransit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyUserOnComplaintApprovedConsumer : IConsumer<ComplaintApprovedEvent>
{
    private readonly INotificationService _notificationService;

    public NotifyUserOnComplaintApprovedConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<ComplaintApprovedEvent> context)
    {
        var body = $"Вашу скаргу на {GetTargetLabel(context.Message.TargetType)} «{context.Message.TargetTitle}» розглянуто та підтверджено.";

        if (!string.IsNullOrWhiteSpace(context.Message.ModerationNote))
        {
            body += $" Примітка модератора: {context.Message.ModerationNote}";
        }

        return _notificationService.SendToUserAsync(
            context.Message.UserId,
            NotificationCategory.System,
            "complaint_approved",
            "Скаргу розглянуто",
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

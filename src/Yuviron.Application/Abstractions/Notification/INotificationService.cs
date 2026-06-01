using Yuviron.Domain.Enums;

namespace Yuviron.Application.Abstractions.Services;

public interface INotificationService
{
    Task SendToUserAsync(
        Guid userId, 
        NotificationCategory category,
        string type,
        string title, 
        string body, 
        NotificationEntityType? entityType = null, 
        Guid? entityId = null, 
        CancellationToken cancellationToken = default);

    Task SendToUsersAsync(
        IEnumerable<Guid> userIds, 
        NotificationCategory category,
        string type,
        string title, 
        string body, 
        NotificationEntityType? entityType = null, 
        Guid? entityId = null, 
        CancellationToken cancellationToken = default);
}
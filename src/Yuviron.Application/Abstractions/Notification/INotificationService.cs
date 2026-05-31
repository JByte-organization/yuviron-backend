using Yuviron.Domain.Enums;

namespace Yuviron.Application.Abstractions.Services;

public interface INotificationService
{
    // Для одиночного уведомления (например, апрув заявки)
    Task SendToUserAsync(
        Guid userId, 
        string title, 
        string body, 
        NotificationEntityType? entityType = null, 
        Guid? entityId = null, 
        CancellationToken cancellationToken = default);

    // Для массовых уведомлений (например, новый релиз всем подписчикам)
    Task SendToUsersAsync(
        IEnumerable<Guid> userIds, 
        string title, 
        string body, 
        NotificationEntityType? entityType = null, 
        Guid? entityId = null, 
        CancellationToken cancellationToken = default);
}
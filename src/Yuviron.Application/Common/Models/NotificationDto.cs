namespace Yuviron.Application.Common.Models;

public record NotificationDto(
    Guid Id,
    string Title,
    string Body,
    string? EntityType, 
    Guid? EntityId,
    bool IsRead,      
    DateTime CreatedAt
);
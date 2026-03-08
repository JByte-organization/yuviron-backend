using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums; // <-- Подключили енамку

namespace Yuviron.Domain.Entities;

public sealed class Notification : Entity // <-- Добавили sealed
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;

    public NotificationEntityType? EntityType { get; private set; }
    public Guid? EntityId { get; private set; }

    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = null!; // <-- Убрали virtual за ненадобностью

    private Notification() { }

    public static Notification Create(
        Guid userId, 
        string title, 
        string body, 
        NotificationEntityType? entityType, 
        Guid? entityId, 
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Notification title is required");
        if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("Notification body is required");

        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title.Trim(),
            Body = body.Trim(),
            EntityType = entityType,
            EntityId = entityId,
            IsRead = false,
            CreatedAt = utcNow
        };
    }

    public void MarkAsRead()
    {
        if (!IsRead) IsRead = true;
    }
}
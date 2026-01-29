using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public enum NotificationEntityType { Track = 1, Album = 2, Artist = 3, System = 99 }

public class Notification : Entity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;

    public NotificationEntityType? EntityType { get; private set; }
    public Guid? EntityId { get; private set; }

    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;

    private Notification() { }
}

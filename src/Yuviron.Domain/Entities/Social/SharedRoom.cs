using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public enum RoomStatus { Active = 1, Closed = 2 }

public class SharedRoom : Entity
{
    public Guid HostUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public RoomStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }

    public virtual User HostUser { get; private set; } = null!;
    public virtual ICollection<SharedRoomMember> Members { get; private set; } = new List<SharedRoomMember>();
    public virtual ICollection<SharedRoomQueueItem> Queue { get; private set; } = new List<SharedRoomQueueItem>();

    private SharedRoom() { }
}

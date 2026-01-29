using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class SharedRoomQueueItem : Entity
{
    public Guid RoomId { get; private set; }
    public Guid TrackId { get; private set; }
    public int Position { get; private set; }
    public Guid AddedByUserId { get; private set; }
    public DateTime AddedAt { get; private set; }

    public virtual SharedRoom Room { get; private set; } = null!;
    public virtual Track Track { get; private set; } = null!;
    public virtual User AddedByUser { get; private set; } = null!;

    private SharedRoomQueueItem() { }
}

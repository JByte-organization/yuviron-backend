using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public enum QueueType { Next = 1, Normal = 2 }

public class PlaybackQueueItem : Entity
{
    public Guid SessionId { get; private set; }
    public Guid TrackId { get; private set; }
    public QueueType QueueType { get; private set; }
    public int Position { get; private set; }

    public DateTime AddedAt { get; private set; }
    public Guid AddedByUserId { get; private set; }

    public virtual PlaybackSession Session { get; private set; } = null!;
    public virtual Track Track { get; private set; } = null!;
    public virtual User AddedByUser { get; private set; } = null!;

    private PlaybackQueueItem() { }
}

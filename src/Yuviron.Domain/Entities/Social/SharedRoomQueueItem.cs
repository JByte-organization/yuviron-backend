using System;
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

    public static SharedRoomQueueItem Create(Guid roomId, Guid trackId, int position, Guid addedByUserId, DateTime utcNow)
    {
        return new SharedRoomQueueItem
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            TrackId = trackId,
            Position = position,
            AddedByUserId = addedByUserId,
            AddedAt = utcNow
        };
    }

    public void UpdatePosition(int newPosition)
    {
        if (newPosition < 0) throw new ArgumentException("Position cannot be negative");
        Position = newPosition;
    }
}
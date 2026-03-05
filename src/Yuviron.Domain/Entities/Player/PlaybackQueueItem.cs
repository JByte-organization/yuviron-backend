using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;


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

    public static PlaybackQueueItem Create(
        Guid sessionId, 
        Guid trackId, 
        QueueType queueType, 
        int position, 
        Guid addedByUserId, 
        DateTime utcNow)
    {
        if (position < 0) throw new ArgumentException("Queue position cannot be negative");

        return new PlaybackQueueItem
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            TrackId = trackId,
            QueueType = queueType,
            Position = position,
            AddedByUserId = addedByUserId,
            AddedAt = utcNow
        };
    }

    public void UpdatePosition(int newPosition)
    {
        if (newPosition < 0) throw new ArgumentException("Queue position cannot be negative");
        Position = newPosition;
    }
}
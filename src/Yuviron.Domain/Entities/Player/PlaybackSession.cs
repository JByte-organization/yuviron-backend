using System;
using System.Collections.Generic;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class PlaybackSession : Entity
{
    public Guid UserId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }

    public string ContextType { get; private set; } = string.Empty;
    public Guid? ContextId { get; private set; } 

    public virtual User User { get; private set; } = null!;
    public virtual ICollection<PlaybackQueueItem> QueueItems { get; private set; } = new List<PlaybackQueueItem>();

    private PlaybackSession() { }

    public static PlaybackSession Create(
        Guid userId, 
        string contextType, 
        Guid? contextId, 
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(contextType)) throw new ArgumentException("Context type is required");

        return new PlaybackSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ContextType = contextType.Trim(),
            ContextId = contextId,
            StartedAt = utcNow
        };
    }

    public void EndSession(DateTime utcNow)
    {
        if (EndedAt != null) return; 
        EndedAt = utcNow;
    }

    public void AddToQueue(Guid trackId, QueueType queueType, int position, Guid addedByUserId, DateTime utcNow)
    {
        if (EndedAt != null) throw new InvalidOperationException("Cannot add items to an ended session");
        
        if (QueueItems.Any(q => q.Position == position))
        {
            throw new InvalidOperationException($"Position {position} is already taken in the queue.");
        }

        var queueItem = PlaybackQueueItem.Create(Id, trackId, queueType, position, addedByUserId, utcNow);
        QueueItems.Add(queueItem);
    }

    public void ClearQueue()
    {
        QueueItems.Clear();
    }
}
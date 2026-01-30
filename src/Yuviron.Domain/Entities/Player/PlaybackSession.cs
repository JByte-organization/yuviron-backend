using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class PlaybackSession : Entity
{
    public Guid UserId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }

    // Контекст: откуда играет (Playlist, Album, Artist)
    public string ContextType { get; private set; } = string.Empty;
    public Guid? ContextId { get; private set; } // ID плейлиста/альбома

    public virtual User User { get; private set; } = null!;
    public virtual ICollection<PlaybackQueueItem> QueueItems { get; private set; } = new List<PlaybackQueueItem>();

    private PlaybackSession() { }
}

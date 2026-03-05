using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class UserSavedTrack
{
    public Guid UserId { get; private set; }
    public Guid TrackId { get; private set; }
    public DateTime SavedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual Track Track { get; private set; } = null!;

    private UserSavedTrack() { }

    public static UserSavedTrack Create(Guid userId, Guid trackId, DateTime utcNow)
    {
        return new UserSavedTrack
        {
            UserId = userId,
            TrackId = trackId,
            SavedAt = utcNow
        };
    }
}
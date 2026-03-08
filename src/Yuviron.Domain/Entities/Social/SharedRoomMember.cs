using System;
using Yuviron.Domain.Enums;
namespace Yuviron.Domain.Entities;

public class SharedRoomMember
{
    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; }
    public RoomRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }

    public virtual SharedRoom Room { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;

    private SharedRoomMember() { }

    public SharedRoomMember(Guid roomId, Guid userId, RoomRole role, DateTime utcNow)
    {
        RoomId = roomId;
        UserId = userId;
        Role = role;
        JoinedAt = utcNow;
    }

    public void Leave(DateTime utcNow)
    {
        if (LeftAt != null) return;
        LeftAt = utcNow;
    }
}
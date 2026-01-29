using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public enum RoomRole { Host = 1, Member = 2 }

public class SharedRoomMember
{
    public Guid RoomId { get; set; }
    public virtual SharedRoom Room { get; set; } = null!;

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public RoomRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

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

    public static SharedRoom Create(Guid hostUserId, string name, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Room name is required");

        var room = new SharedRoom
        {
            Id = Guid.NewGuid(),
            HostUserId = hostUserId,
            Name = name.Trim(),
            Status = RoomStatus.Active,
            CreatedAt = utcNow
        };

        // Создатель автоматически становится хостом в списке участников
        room.Join(hostUserId, RoomRole.Host, utcNow);

        return room;
    }

    public void Close(DateTime utcNow)
    {
        if (Status == RoomStatus.Closed) return;
        
        Status = RoomStatus.Closed;
        EndedAt = utcNow;
        
        // Автоматически "выгоняем" всех, кто еще не вышел
        foreach (var member in Members.Where(m => m.LeftAt == null))
        {
            member.Leave(utcNow);
        }
    }

    public void Join(Guid userId, RoomRole role, DateTime utcNow)
    {
        if (Status == RoomStatus.Closed) throw new InvalidOperationException("Cannot join a closed room");
        
        // Если юзер уже в комнате, ничего не делаем
        if (Members.Any(m => m.UserId == userId && m.LeftAt == null)) return;

        Members.Add(SharedRoomMember.Create(Id, userId, role, utcNow));
    }

    public void EnqueueTrack(Guid trackId, int position, Guid addedByUserId, DateTime utcNow)
    {
        if (Status == RoomStatus.Closed) throw new InvalidOperationException("Cannot add tracks to a closed room");
        if (position < 0) throw new ArgumentException("Position cannot be negative");

        Queue.Add(SharedRoomQueueItem.Create(Id, trackId, position, addedByUserId, utcNow));
    }
}
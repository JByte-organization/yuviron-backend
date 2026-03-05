using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class UserBlock : Entity
{
    public Guid UserId { get; private set; }
    public Guid BlockedByAdminId { get; private set; }
    
    public BlockType BlockType { get; private set; }
    public string ReasonCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual User BlockedByAdmin { get; private set; } = null!;

    private UserBlock() { }

    public static UserBlock Create(
        Guid userId, 
        BlockType type, 
        string reasonCode, 
        string description, 
        Guid adminId, 
        DateTime startsAt, 
        DateTime? endsAt, 
        DateTime utcNow)
    {
        return new UserBlock
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BlockType = type,
            ReasonCode = reasonCode,
            Description = description,
            BlockedByAdminId = adminId,
            StartsAt = startsAt,
            EndsAt = endsAt,
            IsActive = true,
            CreatedAt = utcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
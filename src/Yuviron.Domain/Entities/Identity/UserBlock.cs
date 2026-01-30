using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class UserBlock : Entity
{
    public Guid UserId { get; private set; }
    public BlockType BlockType { get; private set; }
    public string ReasonCode { get; private set; } = string.Empty; // Например: "tos_violation"
    public string Description { get; private set; } = string.Empty;
    public Guid BlockedByAdminId { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual User BlockedByAdmin { get; private set; } = null!;

    private UserBlock() { }
}

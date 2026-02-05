using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class UserProfile : Entity
{
    public Guid UserId { get; private set; } // PK + FK
    public string DisplayName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string? Country { get; private set; }
    public string? Bio { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public virtual User User { get; private set; } = null!;

    private UserProfile() { }

    public static UserProfile Create(Guid userId, string displayName, DateTime dateOfBirth, Gender gender)
    {
        return new UserProfile
        {
            Id = userId,
            UserId = userId,
            DisplayName = displayName,
            DateOfBirth = dateOfBirth,
            Gender = gender,        
            UpdatedAt = DateTime.UtcNow
        };
    }
}

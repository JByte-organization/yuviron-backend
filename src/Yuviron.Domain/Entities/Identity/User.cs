using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public AccountState AccountState { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    // Ссылка на профиль (1:1)
    public virtual UserProfile? Profile { get; private set; }
    public virtual UserSettings? Settings { get; private set; }

    private User() { } 

    public static User Create(string email, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            AccountState = AccountState.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void SetProfile(UserProfile profile)
    {
        Profile = profile;
    }
}

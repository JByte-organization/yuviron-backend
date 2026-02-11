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
    public bool AcceptMarketing { get; private set; }
    public bool AcceptTerms { get; private set; }
    public string? LoginCodeHash { get; private set; }
    public DateTime? LoginCodeExpiryUtc { get; private set; }

    public virtual ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    // Ссылка на профиль (1:1)
    public virtual UserProfile? Profile { get; private set; }
    public virtual UserSettings? Settings { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash, bool acceptMarketing, bool acceptTerms)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            AccountState = AccountState.Active,
            AcceptMarketing = acceptMarketing,
            AcceptTerms = acceptTerms,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public bool HasActivePremiumSubscription(DateTime currentDate)
    {
        return Subscriptions.Any(s =>
            s.Status == SubscriptionStatus.Active &&
            s.EndAt > currentDate);
    }

    public void SetProfile(UserProfile profile)
    {
        Profile = profile;
    }

    public void SetLoginCode(string codeHash)
    {
        LoginCodeHash = codeHash;
        LoginCodeExpiryUtc = DateTime.UtcNow.AddMinutes(10);
    }

    public void ClearLoginCode()
    {
        LoginCodeHash = null;
        LoginCodeExpiryUtc = null;
    }
}

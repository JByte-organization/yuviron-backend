using System;
using System.Collections.Generic;
using System.Linq;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

// using Yuviron.Domain.Events; // На будущее

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

    // --- Добавили Soft Delete ---
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public virtual ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    public virtual UserProfile? Profile { get; private set; }
    public virtual UserSettings? Settings { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash, bool acceptMarketing, bool acceptTerms, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required");

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLower().Trim(),
            PasswordHash = passwordHash,
            AccountState = AccountState.Active,
            AcceptMarketing = acceptMarketing,
            AcceptTerms = acceptTerms,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            IsDeleted = false
        };
    }

    public void SyncRoles(IEnumerable<Guid> roleIds)
    {
        var newIds = roleIds.Distinct().ToList();

        var toRemove = UserRoles.Where(ur => !newIds.Contains(ur.RoleId)).ToList();
        foreach (var item in toRemove) UserRoles.Remove(item);

        var currentIds = UserRoles.Select(ur => ur.RoleId).ToList();
        foreach (var id in newIds.Where(id => !currentIds.Contains(id)))
        {
            UserRoles.Add(UserRole.Create(Id, id));
        }
    }

    public void UpdateLastLogin(DateTime utcNow)
    {
        LastLoginAt = utcNow;
        UpdatedAt = utcNow;
    }

    public bool HasActivePremiumSubscription(DateTime currentDate)
    {
        return Subscriptions.Any(s =>
            s.Status == SubscriptionStatus.Active &&
            s.EndAt > currentDate);
    }

    public void SetProfile(UserProfile profile) => Profile = profile;

    public void UpdateAdminDetails(string email, bool acceptMarketing, bool acceptTerms, AccountState accountState, DateTime utcNow)
    {
        Email = email;
        AcceptMarketing = acceptMarketing;
        AcceptTerms = acceptTerms;
        AccountState = accountState;
        UpdatedAt = utcNow;
    }

    public void SetPasswordHash(string passwordHash, DateTime utcNow)
    {
        PasswordHash = passwordHash;
        UpdatedAt = utcNow;
    }

    public void SetLoginCode(string codeHash, DateTime utcNow)
    {
        LoginCodeHash = codeHash;
        LoginCodeExpiryUtc = utcNow.AddMinutes(10);
    }

    public void ClearLoginCode()
    {
        LoginCodeHash = null;
        LoginCodeExpiryUtc = null;
    }

    // --- Метод для мягкого удаления ---
    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = utcNow;
        UpdatedAt = utcNow;
        
        AddDomainEvent(new UserDeletedEvent(this.Id));
    }
}
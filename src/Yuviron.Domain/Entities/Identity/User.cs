using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

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
    public bool IsEmailConfirmed { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public virtual ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public virtual ICollection<ArtistTeamMember> ManagedArtists { get; private set; } = new List<ArtistTeamMember>();
    public virtual UserProfile Profile { get; private set; } = null!;
    public virtual UserSettings? Settings { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash, string firstName, bool acceptMarketing, bool acceptTerms, DateTime utcNow, AccountState accountState = AccountState.Active)
    {

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            AccountState = accountState,
            AcceptMarketing = acceptMarketing,
            AcceptTerms = acceptTerms,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            IsDeleted = false
        };

        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email, firstName));
        
        return user;
    }

    public void SyncRoles(IEnumerable<Guid> roleIds)
    {
        var newIds = roleIds.Distinct().ToList();

        var toRemove = UserRoles.Where(ur => !newIds.Contains(ur.RoleId)).ToList();
        foreach (var item in toRemove) UserRoles.Remove(item);

        var currentIds = UserRoles.Select(ur => ur.RoleId).ToList();
        foreach (var id in newIds.Where(id => !currentIds.Contains(id)))
        {
            UserRoles.Add(new UserRole(Id, id));
        }
    }

    public void ConfirmEmail(string firstName, DateTime utcNow)
    {
        if (IsEmailConfirmed) return;
        
        IsEmailConfirmed = true;
        UpdatedAt = utcNow;

        AddDomainEvent(new UserEmailConfirmedEvent(this.Id, this.Email, firstName));
    }
    
    public void RequestPasswordReset(string token, string firstName, DateTime utcNow)
    {
        UpdatedAt = utcNow; 
        AddDomainEvent(new ForgotPasswordRequestedEvent(this.Id, this.Email, firstName, token));
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

    public void UpdateAdminDetails(string email, bool acceptMarketing, AccountState accountState, DateTime utcNow)
    {
        Email = EmailNormalizer.Normalize(email);
        AcceptMarketing = acceptMarketing;
        AccountState = accountState;
        UpdatedAt = utcNow;
    }

    public void SetPasswordHash(string passwordHash, DateTime utcNow)
    {
        PasswordHash = passwordHash;
        UpdatedAt = utcNow;
        
        AddDomainEvent(new UserPasswordChangedEvent(this.Id));
    }

    public void SetAccountState(AccountState newState, DateTime utcNow)
    {
        if (AccountState == newState) return;
        
        AccountState = newState;
        UpdatedAt = utcNow;
        
        AddDomainEvent(new UserPermissionsChangedEvent(this.Id));
    }
    
    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = utcNow;
        UpdatedAt = utcNow;
        AccountState = AccountState.Deleted;

        var suffix = $"_del_{Id.ToString()[..8]}"; 
        Email = $"{Email[..Math.Min(Email.Length, 320 - suffix.Length)]}{suffix}";
        PasswordHash = "deleted_account_hash_invalidated";

        AddDomainEvent(new UserDeletedEvent(this.Id));
    }
    public void UpdateMarketingPreferences(bool acceptMarketing, DateTime utcNow)
    {
        AcceptMarketing = acceptMarketing;
        UpdatedAt = utcNow;
    }

    public void UpdateAccountDetails(string email, bool acceptMarketing, DateTime utcNow)
    {
        Email = EmailNormalizer.Normalize(email);
        AcceptMarketing = acceptMarketing;
        UpdatedAt = utcNow;
    }
}

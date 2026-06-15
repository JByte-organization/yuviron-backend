using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Domain.Entities;

public class Artist : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; }
    
    public long TotalPlays { get; private set; }
    public int MonthlyListenersCount { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; } 

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public virtual ICollection<ArtistTeamMember> TeamMembers { get; private set; } = new List<ArtistTeamMember>();

    public virtual ICollection<ArtistSocialLink> SocialLinks { get; private set; } = new List<ArtistSocialLink>();
    public virtual ICollection<ArtistPin> Pins { get; private set; } = new List<ArtistPin>();
    public virtual ICollection<AlbumArtist> AlbumArtists { get; private set; } = new List<AlbumArtist>();
    public virtual ICollection<TrackArtist> TrackArtists { get; private set; } = new List<TrackArtist>();
    public virtual ICollection<ArtistSubscription> Subscriptions { get; private set; } = new List<ArtistSubscription>();
    public virtual ArtistWallet? ArtistWallet { get; private set; }
    public virtual ArtistPayoutSettings? PayoutSettings { get; private set; }

    private Artist() { }

    public static Artist Create(
        Guid? initialOwnerUserId, 
        string name,
        string? bio,
        string? avatarUrl,
        string? bannerUrl,
        VerificationStatus verificationStatus,
        DateTime utcNow)
    {
        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Bio = bio?.Trim(),
            AvatarUrl = avatarUrl?.Trim(),
            BannerUrl = bannerUrl?.Trim(),
            VerificationStatus = verificationStatus,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            IsDeleted = false,
            TotalPlays = 0,
            MonthlyListenersCount = 0
        };

        if (initialOwnerUserId.HasValue)
        {
            artist.TeamMembers.Add(ArtistTeamMember.Create(artist.Id, initialOwnerUserId.Value, ArtistTeamRole.Owner, utcNow));
        }

        return artist;
    }

    public void UpdateDetails(
        string name,
        string? bio,
        string? avatarUrl,
        string? bannerUrl,
        VerificationStatus verificationStatus,
        DateTime utcNow)
    {
        Name = name.Trim();
        Bio = bio?.Trim();
        AvatarUrl = avatarUrl?.Trim();
        BannerUrl = bannerUrl?.Trim();
        VerificationStatus = verificationStatus;
        UpdatedAt = utcNow;
    }

    public void AddTeamMember(Guid userId, ArtistTeamRole role, DateTime utcNow)
    {
        if (role == ArtistTeamRole.Owner && TeamMembers.Any(tm => tm.Role == ArtistTeamRole.Owner))
        {
            throw new InvalidOperationException("An artist can have only one owner.");
        }

        var existingMember = TeamMembers.FirstOrDefault(tm => tm.UserId == userId);
    
        if (existingMember != null)
        {
            existingMember.ChangeRole(role);
        }
        else
        {
            TeamMembers.Add(ArtistTeamMember.Create(this.Id, userId, role, utcNow));
        }
    
        AddDomainEvent(new UserPermissionsChangedEvent(userId));
        UpdatedAt = utcNow;
    }

    public void UpdateTeamMemberRole(Guid targetUserId, ArtistTeamRole newRole, DateTime utcNow)
    {
        var targetMember = TeamMembers.FirstOrDefault(tm => tm.UserId == targetUserId);
        if (targetMember == null)
        {
            throw new InvalidOperationException("User is not in the team.");
        }

        if (newRole == ArtistTeamRole.Owner && targetMember.Role != ArtistTeamRole.Owner)
        {
            var currentOwner = TeamMembers.FirstOrDefault(tm => tm.Role == ArtistTeamRole.Owner);
            
            if (currentOwner != null)
            {
                currentOwner.ChangeRole(ArtistTeamRole.Manager);
                AddDomainEvent(new UserPermissionsChangedEvent(currentOwner.UserId));
            }
        }

        targetMember.ChangeRole(newRole);
        AddDomainEvent(new UserPermissionsChangedEvent(targetMember.UserId));
        
        UpdatedAt = utcNow;
    }

    public bool RemoveTeamMember(Guid userId, DateTime utcNow)
    {
        var member = TeamMembers.FirstOrDefault(tm => tm.UserId == userId);
        if (member != null)
        {
            if (member.Role == ArtistTeamRole.Owner)
            {
                throw new InvalidOperationException("Cannot remove the owner. Transfer ownership to another member first.");
            }

            TeamMembers.Remove(member);
            AddDomainEvent(new UserPermissionsChangedEvent(userId));
            UpdatedAt = utcNow;
            return true; 
        }
    
        return false; 
    }
    
    public void AddPlays(long count)
    {
        if (count > 0) 
        {
            TotalPlays += count;
        }
    }

    public void SetMonthlyListenersCount(int count)
    {
        MonthlyListenersCount = Math.Max(0, count);
    }
    
    public bool HasActivePremiumSubscription(DateTime currentDate)
    {
        return Subscriptions.Any(s =>
            s.Status == SubscriptionStatus.Active &&
            s.EndAt > currentDate);
    }
    
    public void AddSocialLink(SocialLinkType type, string url, DateTime utcNow)
    {
        // Проверка на дубликаты осталась
        if (SocialLinks.Any(l => l.Type == type))
        {
            throw new SocialLinkAlreadyExistsException(type);
        }

        // Просто добавляем новую связь
        SocialLinks.Add(ArtistSocialLink.Create(this.Id, type, url, utcNow));
    
        // МЫ ПОЛНОСТЬЮ УБРАЛИ UpdatedAt = utcNow;
        // Теперь EF Core даже не попытается обновить таблицу artists,
        // а значит, MySQL не сможет выдать ошибку "0 строк".
    }

    public void RemoveSocialLink(SocialLinkType type, DateTime utcNow)
    {
        var link = SocialLinks.FirstOrDefault(l => l.Type == type);
        if (link != null)
        {
            // Просто удаляем связь
            SocialLinks.Remove(link);
        
            // МЫ ПОЛНОСТЬЮ УБРАЛИ UpdatedAt = utcNow;
        }
    }

    public void SetPin(ArtistPinType type, Guid entityId, int position, DateTime utcNow)
    {
        var existingPin = Pins.FirstOrDefault(p => p.Position == position);
        if (existingPin != null)
        {
            Pins.Remove(existingPin);
        }

        Pins.Add(ArtistPin.Create(this.Id, type, entityId, position, utcNow));
        UpdatedAt = utcNow;
    }

    public void RemovePin(int position, DateTime utcNow)
    {
        var existingPin = Pins.FirstOrDefault(p => p.Position == position);
        if (existingPin != null)
        {
            Pins.Remove(existingPin);
            UpdatedAt = utcNow;
        }
    }

    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = utcNow;
        UpdatedAt = utcNow;
        
        AddDomainEvent(new ArtistDeletedEvent(this.Id));

        if (!string.IsNullOrWhiteSpace(AvatarUrl))
        {
            AddDomainEvent(new FileNeedsDeletionEvent(AvatarUrl));
        }

        if (!string.IsNullOrWhiteSpace(BannerUrl))
        {
            AddDomainEvent(new FileNeedsDeletionEvent(BannerUrl));
        }
    }
}
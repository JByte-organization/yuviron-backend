using System;
using System.Collections.Generic;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Domain.Entities;

public class Artist : Entity
{
    public Guid? OwnerUserId { get; private set; } 
    public string Name { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public bool IsVerified { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; } 

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public virtual User? OwnerUser { get; private set; }
    public virtual ICollection<ArtistSocialLink> SocialLinks { get; private set; } = new List<ArtistSocialLink>();
    public virtual ICollection<ArtistPin> Pins { get; private set; } = new List<ArtistPin>();
    public virtual ICollection<AlbumArtist> AlbumArtists { get; private set; } = new List<AlbumArtist>();
    public virtual ICollection<TrackArtist> TrackArtists { get; private set; } = new List<TrackArtist>();

    private Artist() { }

    public static Artist Create(
        Guid? ownerUserId,
        string name,
        string? bio,
        string? avatarUrl,
        string? bannerUrl,
        VerificationStatus verificationStatus,
        DateTime utcNow)
    {
        return new Artist
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            Name = name.Trim(),
            Bio = bio?.Trim(),
            AvatarUrl = avatarUrl?.Trim(),
            BannerUrl = bannerUrl?.Trim(),
            VerificationStatus = verificationStatus,
            IsVerified = verificationStatus == VerificationStatus.Verified,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            IsDeleted = false
        };
    }

    public void UpdateDetails(
        Guid? ownerUserId,
        string name,
        string? bio,
        string? avatarUrl,
        string? bannerUrl,
        VerificationStatus verificationStatus,
        DateTime utcNow)
    {
        OwnerUserId = ownerUserId;
        Name = name.Trim();
        Bio = bio?.Trim();
        AvatarUrl = avatarUrl?.Trim();
        BannerUrl = bannerUrl?.Trim();
        VerificationStatus = verificationStatus;
        IsVerified = verificationStatus == VerificationStatus.Verified;
        UpdatedAt = utcNow;
    }

    // ВЕРНУЛ МЕТОД DELETE ВНУТРЬ КЛАССА
    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = utcNow;
        UpdatedAt = utcNow;
        
        AddDomainEvent(new ArtistDeletedEvent(this.Id));
    }
}
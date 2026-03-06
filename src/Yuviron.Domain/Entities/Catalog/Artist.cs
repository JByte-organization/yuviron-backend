using System;
using System.Collections.Generic;
using System.Linq;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Domain.Entities;

public class Artist : Entity
{
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

    public virtual ICollection<ArtistTeamMember> TeamMembers { get; private set; } = new List<ArtistTeamMember>();

    public virtual ICollection<ArtistSocialLink> SocialLinks { get; private set; } = new List<ArtistSocialLink>();
    public virtual ICollection<ArtistPin> Pins { get; private set; } = new List<ArtistPin>();
    public virtual ICollection<AlbumArtist> AlbumArtists { get; private set; } = new List<AlbumArtist>();
    public virtual ICollection<TrackArtist> TrackArtists { get; private set; } = new List<TrackArtist>();

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
            IsVerified = verificationStatus == VerificationStatus.Verified,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            IsDeleted = false
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
        IsVerified = verificationStatus == VerificationStatus.Verified;
        UpdatedAt = utcNow;
    }

    public void AddTeamMember(Guid userId, ArtistTeamRole role, DateTime utcNow)
    {
        var existingMember = TeamMembers.FirstOrDefault(tm => tm.UserId == userId);
        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already in the team."); 
        }

        TeamMembers.Add(ArtistTeamMember.Create(this.Id, userId, role, utcNow));
        UpdatedAt = utcNow;
    }

    public void UpdateTeamMemberRole(Guid userId, ArtistTeamRole newRole, DateTime utcNow)
    {
        var member = TeamMembers.FirstOrDefault(tm => tm.UserId == userId);
        if (member == null)
        {
            throw new InvalidOperationException("User is not in the team.");
        }

        member.ChangeRole(newRole);
        UpdatedAt = utcNow;
    }

    public void RemoveTeamMember(Guid userId, DateTime utcNow)
    {
        var member = TeamMembers.FirstOrDefault(tm => tm.UserId == userId);
        if (member != null)
        {
            TeamMembers.Remove(member);
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
    }
}
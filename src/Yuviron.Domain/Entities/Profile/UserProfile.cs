using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Domain.Entities;

public class UserProfile : Entity
{
    public string FirstName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string? BannerUrl { get; private set; } 
    public string? Country { get; private set; }
    public string? City { get; private set; }
    public string? Bio { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    
    public virtual User User { get; private set; } = null!;

    private UserProfile() { }

    public static UserProfile Create(
        Guid userId, 
        string firstName, 
        string? avatarUrl, 
        string? bannerUrl, 
        string? country, 
        string? city,
        string? bio, 
        DateTime dateOfBirth, 
        Gender gender, 
        DateTime utcNow)
    {
        return new UserProfile
        {
            Id = userId,
            FirstName = firstName.Trim(),
            AvatarUrl = avatarUrl?.Trim(),
            BannerUrl = bannerUrl?.Trim(),
            Country = country?.Trim(),
            City = city?.Trim(),
            Bio = bio?.Trim(),
            DateOfBirth = dateOfBirth,
            Gender = gender,        
            UpdatedAt = utcNow
        };
    }

    public void UpdateDetails(
        string firstName, 
        string? avatarUrl, 
        string? bannerUrl, 
        string? country, 
        string? city,
        string? bio, 
        DateTime dateOfBirth, 
        Gender gender, 
        DateTime utcNow)
    {
        FirstName = firstName.Trim();
        AvatarUrl = avatarUrl?.Trim();
        BannerUrl = bannerUrl?.Trim();
        Country = country?.Trim();
        City = city?.Trim();
        Bio = bio?.Trim();
        DateOfBirth = dateOfBirth;
        Gender = gender;
        UpdatedAt = utcNow;
    }

    public void UpdateAccountDetails(
        string? country,
        DateTime dateOfBirth,
        Gender gender,
        DateTime utcNow)
    {
        Country = country?.Trim();
        DateOfBirth = dateOfBirth;
        Gender = gender;
        UpdatedAt = utcNow;
    }
    
    public void ClearPersonalData(DateTime utcNow)
    {
        if (!string.IsNullOrWhiteSpace(AvatarUrl))
        {
            AddDomainEvent(new FileNeedsDeletionEvent(AvatarUrl));
        }
        
        if (!string.IsNullOrWhiteSpace(BannerUrl))
        {
            AddDomainEvent(new FileNeedsDeletionEvent(BannerUrl));
        }

        FirstName = "Deleted User";
        AvatarUrl = null;
        BannerUrl = null;
        Country = null;
        City = null;
        Bio = null;
        DateOfBirth = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Gender = default; 
        UpdatedAt = utcNow;
    }
}

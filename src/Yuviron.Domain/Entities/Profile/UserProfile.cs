using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class UserProfile : Entity
{
    
    public string DisplayName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string? Country { get; private set; }
    public string? Bio { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    
    public virtual User User { get; private set; } = null!;

    private UserProfile() { }

    public static UserProfile Create(
        Guid userId, 
        string displayName, 
        string? avatarUrl, 
        string? country, 
        string? bio, 
        DateTime dateOfBirth, 
        Gender gender, 
        DateTime utcNow)
    {
        return new UserProfile
        {
            Id = userId,
            DisplayName = displayName.Trim(),
            AvatarUrl = avatarUrl?.Trim(),
            Country = country?.Trim(),
            Bio = bio?.Trim(),
            DateOfBirth = dateOfBirth,
            Gender = gender,        
            UpdatedAt = utcNow
        };
    }

    public void UpdateDetails(
        string displayName, 
        string? avatarUrl, 
        string? country, 
        string? bio, 
        DateTime dateOfBirth, 
        Gender gender, 
        DateTime utcNow)
    {
        DisplayName = displayName.Trim();
        AvatarUrl = avatarUrl?.Trim();
        Country = country?.Trim();
        Bio = bio?.Trim();
        DateOfBirth = dateOfBirth;
        Gender = gender;
        UpdatedAt = utcNow;
    }
}
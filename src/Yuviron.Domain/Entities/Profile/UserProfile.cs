using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Domain.Entities;

public class UserProfile : Entity
{
    
    public string FirstName { get; private set; } = string.Empty;
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
        string firstName, 
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
            FirstName = firstName.Trim(),
            AvatarUrl = avatarUrl?.Trim(),
            Country = country?.Trim(),
            Bio = bio?.Trim(),
            DateOfBirth = dateOfBirth,
            Gender = gender,        
            UpdatedAt = utcNow
        };
    }

    public void UpdateDetails(
        string firstName, 
        string? avatarUrl, 
        string? country, 
        string? bio, 
        DateTime dateOfBirth, 
        Gender gender, 
        DateTime utcNow)
    {
        FirstName = firstName.Trim();
        AvatarUrl = avatarUrl?.Trim();
        Country = country?.Trim();
        Bio = bio?.Trim();
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

        FirstName = "Deleted User";
        AvatarUrl = null;
        Country = null;
        Bio = null;
        DateOfBirth = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Gender = default; 
        UpdatedAt = utcNow;
    }
}
using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class CustomTheme : Entity
{
    public Guid UserId { get; private set; }
    public string PrimaryColor { get; private set; } = string.Empty;
    public string SecondaryColor { get; private set; } = string.Empty;
    public string BackgroundColor { get; private set; } = string.Empty;
    
    public string? BackgroundImageUrl { get; private set; } 
    
    public DateTime CreatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;

    private CustomTheme() { }

    public static CustomTheme Create(
        Guid userId, 
        string primaryColor, 
        string secondaryColor, 
        string backgroundColor, 
        string? backgroundImageUrl,
        DateTime utcNow)
    {
        return new CustomTheme
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PrimaryColor = primaryColor,
            SecondaryColor = secondaryColor,
            BackgroundColor = backgroundColor,
            BackgroundImageUrl = backgroundImageUrl,
            CreatedAt = utcNow
        };
    }
}
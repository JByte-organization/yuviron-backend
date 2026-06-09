using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Theme : Entity
{
    public Guid? UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string PrimaryColor { get; private set; } = string.Empty;
    public string SecondaryColor { get; private set; } = string.Empty;
    public string BackgroundColor { get; private set; } = string.Empty;
    public bool IsSystem { get; private set; }
    public bool IsPremiumOnly { get; private set; }

    public virtual User? User { get; private set; }

    private Theme() { }

    public static Theme Create(
        string name,
        string primaryColor,
        string secondaryColor,
        string backgroundColor,
        bool isSystem,
        bool isPremiumOnly,
        Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Theme name is required");
        if (string.IsNullOrWhiteSpace(primaryColor)) throw new ArgumentException("Primary color is required");
        if (string.IsNullOrWhiteSpace(secondaryColor)) throw new ArgumentException("Secondary color is required");
        if (string.IsNullOrWhiteSpace(backgroundColor)) throw new ArgumentException("Background color is required");

        return new Theme
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            PrimaryColor = primaryColor.Trim(),
            SecondaryColor = secondaryColor.Trim(),
            BackgroundColor = backgroundColor.Trim(),
            IsSystem = isSystem,
            IsPremiumOnly = isPremiumOnly
        };
    }

    public void Update(
        string name,
        string primaryColor,
        string secondaryColor,
        string backgroundColor,
        bool isSystem,
        bool isPremiumOnly)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Theme name is required");
        if (string.IsNullOrWhiteSpace(primaryColor)) throw new ArgumentException("Primary color is required");
        if (string.IsNullOrWhiteSpace(secondaryColor)) throw new ArgumentException("Secondary color is required");
        if (string.IsNullOrWhiteSpace(backgroundColor)) throw new ArgumentException("Background color is required");

        Name = name.Trim();
        PrimaryColor = primaryColor.Trim();
        SecondaryColor = secondaryColor.Trim();
        BackgroundColor = backgroundColor.Trim();
        IsSystem = isSystem;
        IsPremiumOnly = isPremiumOnly;
    }
}

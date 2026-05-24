using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Theme : Entity
{
    public string Name { get; private set; } = string.Empty;
    public bool IsSystem { get; private set; }
    public bool IsPremiumOnly { get; private set; }

    private Theme() { }

    public static Theme Create(string name, bool isSystem, bool isPremiumOnly)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Theme name is required");

        return new Theme
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            IsSystem = isSystem,
            IsPremiumOnly = isPremiumOnly
        };
    }
}
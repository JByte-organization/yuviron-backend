using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Achievement : Entity
{
    public string Code { get; private set; } = string.Empty; 
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private Achievement() { }

    public static Achievement Create(string code, string title, string description, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Achievement code is required");
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required");

        return new Achievement
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToLowerInvariant(), 
            Title = title.Trim(),
            Description = description.Trim(),
            IsActive = isActive
        };
    }

    public void UpdateDetails(string title, string description)
    {
        Title = title.Trim();
        Description = description.Trim();
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
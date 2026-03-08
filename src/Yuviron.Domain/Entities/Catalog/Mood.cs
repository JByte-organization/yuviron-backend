using System;
using System.Collections.Generic;
using Yuviron.Domain.Common;
using Yuviron.Domain.Events;

namespace Yuviron.Domain.Entities;

public class Mood : Entity 
{
    public string Name { get; private set; } = string.Empty;
    public string? CoverUrl { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; } 
    public bool IsDeleted { get; private set; }

    public virtual ICollection<TrackMood> TrackMoods { get; private set; } = new List<TrackMood>();

    private Mood() { }

    public static Mood Create(string name, string? coverUrl, DateTime utcNow)
    {
        return new Mood
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CoverUrl = coverUrl?.Trim(),
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            IsDeleted = false
        };
    }

    public void Update(string name, string? coverUrl, DateTime utcNow)
    {
        Name = name.Trim();
        CoverUrl = coverUrl?.Trim();
        UpdatedAt = utcNow; // Обновляем время
    }

    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        UpdatedAt = utcNow;
        
        var suffix = $"_del_{Id.ToString()[..8]}";
        Name = $"{Name[..Math.Min(Name.Length, 100 - suffix.Length)]}{suffix}";

        AddDomainEvent(new MoodDeletedEvent(this.Id));
    }
}
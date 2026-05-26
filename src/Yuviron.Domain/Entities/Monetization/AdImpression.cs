using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class AdImpression : Entity
{
    public Guid AdId { get; private set; }
    public Guid UserId { get; private set; } 
    public DateTime ShownAt { get; private set; }
    public string? Context { get; private set; } 
    public bool IsClicked { get; private set; } 
    public DateTime? ClickedAt { get; private set; }

    public virtual Ad Ad { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;

    private AdImpression() { }

    public static AdImpression Create(Guid adId, Guid userId, string? context, DateTime utcNow)
    {
        return new AdImpression
        {
            Id = Guid.NewGuid(),
            AdId = adId,
            UserId = userId,
            Context = context?.Trim(),
            ShownAt = utcNow,
            IsClicked = false
        };
    }

    public void MarkAsClicked(DateTime utcNow)
    {
        if (IsClicked) return;
        IsClicked = true;
        ClickedAt = utcNow;
    }
}
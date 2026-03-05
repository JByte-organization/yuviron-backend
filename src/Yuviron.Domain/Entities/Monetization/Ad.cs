using System;
using System.Collections.Generic;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Ad : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string MediaUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public virtual ICollection<AdImpression> Impressions { get; private set; } = new List<AdImpression>();

    private Ad() { }

    public static Ad Create(string title, string mediaUrl, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required");
        if (string.IsNullOrWhiteSpace(mediaUrl)) throw new ArgumentException("Media URL is required");

        return new Ad
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            MediaUrl = mediaUrl.Trim(),
            IsActive = isActive
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
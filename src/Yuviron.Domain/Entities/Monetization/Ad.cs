using System;
using System.Collections.Generic;
using Yuviron.Domain.Common;
using Yuviron.Domain.Events;

namespace Yuviron.Domain.Entities;

public class Ad : Entity
{
    public string AdvertiserName { get; private set; } = string.Empty; 
    public string Title { get; private set; } = string.Empty; 
    public string AudioUrl { get; private set; } = string.Empty; 
    public string ImageUrl { get; private set; } = string.Empty; 
    public string? ClickUrl { get; private set; } 

    public bool IsActive { get; private set; }
    
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual ICollection<AdImpression> Impressions { get; private set; } = new List<AdImpression>();

    private Ad() { }

    public static Ad Create(
        string advertiserName, 
        string title, 
        string audioUrl, 
        string imageUrl, 
        string? clickUrl, 
        bool isActive,
        DateTime utcNow)
    {
        return new Ad
        {
            Id = Guid.NewGuid(),
            AdvertiserName = advertiserName.Trim(),
            Title = title.Trim(),
            AudioUrl = audioUrl,
            ImageUrl = imageUrl,
            ClickUrl = clickUrl?.Trim(),
            IsActive = isActive, 
            IsDeleted = false,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public void Update(
        string advertiserName, 
        string title, 
        string audioUrl, 
        string imageUrl, 
        string? clickUrl, 
        bool isActive, 
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(advertiserName)) throw new ArgumentException("Advertiser name is required");
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required");
        if (string.IsNullOrWhiteSpace(audioUrl)) throw new ArgumentException("Audio URL is required");
        if (string.IsNullOrWhiteSpace(imageUrl)) throw new ArgumentException("Image URL is required");

        AdvertiserName = advertiserName.Trim();
        Title = title.Trim();
        AudioUrl = audioUrl;
        ImageUrl = imageUrl;
        ClickUrl = clickUrl?.Trim();
        IsActive = isActive;
        UpdatedAt = utcNow;
    }

    public void SetActiveStatus(bool isActive, DateTime utcNow)
    {
        IsActive = isActive;
        UpdatedAt = utcNow;
    }

    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        IsActive = false;
        DeletedAt = utcNow;
        UpdatedAt = utcNow;
        
        AddDomainEvent(new FileNeedsDeletionEvent(AudioUrl));
        AddDomainEvent(new FileNeedsDeletionEvent(ImageUrl));
    }
}
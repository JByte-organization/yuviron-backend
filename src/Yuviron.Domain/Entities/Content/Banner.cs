using Yuviron.Domain.Common;
using Yuviron.Domain.Events;

namespace Yuviron.Domain.Entities;

public class Banner : Entity
{
    public string Title { get; private set; } = string.Empty; 
    
    public string BannerUrl { get; private set; } = string.Empty;
    
    public string TargetUrl { get; private set; } = string.Empty;
    
    public int SortOrder { get; private set; }
    
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Banner() { }

    public static Banner Create(string title, string bannerUrl, string targetUrl, int sortOrder, bool isActive, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required");
        if (string.IsNullOrWhiteSpace(bannerUrl)) throw new ArgumentException("Banner image URL is required");
        if (string.IsNullOrWhiteSpace(targetUrl)) throw new ArgumentException("Target URL is required");

        return new Banner
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            BannerUrl = bannerUrl.Trim(),
            TargetUrl = targetUrl.Trim(),
            SortOrder = sortOrder,
            IsActive = isActive,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public void Update(string title, string bannerUrl, string targetUrl, int sortOrder, bool isActive, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required");
        if (string.IsNullOrWhiteSpace(bannerUrl)) throw new ArgumentException("Banner image URL is required");
        if (string.IsNullOrWhiteSpace(targetUrl)) throw new ArgumentException("Target URL is required");

        Title = title.Trim();
        BannerUrl = bannerUrl.Trim();
        TargetUrl = targetUrl.Trim();
        SortOrder = sortOrder;
        IsActive = isActive;
        UpdatedAt = utcNow;
    }

    public void ToggleStatus(DateTime utcNow)
    {
        IsActive = !IsActive;
        UpdatedAt = utcNow;
    }

    public void Delete()
    {
        AddDomainEvent(new FileNeedsDeletionEvent(BannerUrl));
    }
}
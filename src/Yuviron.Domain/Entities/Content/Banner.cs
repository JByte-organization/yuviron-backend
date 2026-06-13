using Yuviron.Domain.Common;
using Yuviron.Domain.Events;

namespace Yuviron.Domain.Entities;

public class Banner : Entity
{
    public Guid? ArtistId { get; private set; }
    public string Title { get; private set; } = string.Empty; 
    
    public string BannerUrl { get; private set; } = string.Empty;
    
    public string TargetUrl { get; private set; } = string.Empty;
    
    public bool IsActive { get; private set; }
    public DateTime? StartsAtUtc { get; private set; }
    public DateTime? EndsAtUtc { get; private set; }
    public DateTime? StartNotificationSentAtUtc { get; private set; }

    public string? TargetCountries { get; private set; }
    public string? TargetGenres { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual Artist? Artist { get; private set; }

    private Banner() { }

    public static Banner Create(
        string title,
        string bannerUrl,
        string targetUrl,
        bool isActive,
        DateTime utcNow,
        Guid? artistId = null,
        DateTime? startsAtUtc = null,
        DateTime? endsAtUtc = null,
        string? targetCountries = null,
        string? targetGenres = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required");
        if (string.IsNullOrWhiteSpace(bannerUrl)) throw new ArgumentException("Banner image URL is required");
        if (string.IsNullOrWhiteSpace(targetUrl)) throw new ArgumentException("Target URL is required");

        return new Banner
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            Title = title.Trim(),
            BannerUrl = bannerUrl.Trim(),
            TargetUrl = targetUrl.Trim(),
            IsActive = isActive,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            TargetCountries = targetCountries,
            TargetGenres = targetGenres,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public void Update(
        string title,
        string bannerUrl,
        string targetUrl,
        bool isActive,
        DateTime utcNow,
        Guid? artistId = null,
        DateTime? startsAtUtc = null,
        DateTime? endsAtUtc = null,
        string? targetCountries = null,
        string? targetGenres = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required");
        if (string.IsNullOrWhiteSpace(bannerUrl)) throw new ArgumentException("Banner image URL is required");
        if (string.IsNullOrWhiteSpace(targetUrl)) throw new ArgumentException("Target URL is required");

        ArtistId = artistId;
        Title = title.Trim();
        BannerUrl = bannerUrl.Trim();
        TargetUrl = targetUrl.Trim();
        IsActive = isActive;
        StartsAtUtc = startsAtUtc ?? StartsAtUtc;
        EndsAtUtc = endsAtUtc ?? EndsAtUtc;
        TargetCountries = targetCountries;
        TargetGenres = targetGenres;
        UpdatedAt = utcNow;
    }

    public void ToggleStatus(DateTime utcNow)
    {
        IsActive = !IsActive;
        UpdatedAt = utcNow;
    }

    public void MarkStartNotificationSent(DateTime utcNow)
    {
        StartNotificationSentAtUtc = utcNow;
        UpdatedAt = utcNow;
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAt = utcNow;
    }

    public void Delete()
    {
        AddDomainEvent(new FileNeedsDeletionEvent(BannerUrl));
    }
}

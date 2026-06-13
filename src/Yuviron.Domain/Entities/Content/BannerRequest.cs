using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using System;

namespace Yuviron.Domain.Entities;

public class BannerRequest : Entity
{
    public Guid ArtistId { get; private set; }
    public Guid SubmittedByUserId { get; private set; }
    public Guid? AlbumId { get; private set; } 
    public string Title { get; private set; } = string.Empty;
    public string? BannerUrl { get; private set; }
    public BannerRequestStatus Status { get; private set; } 
    public string? AdminNotes { get; private set; }
    
    public bool IsPaid { get; private set; }
    public string? StripeSessionId { get; private set; }
    public string? StripePaymentIntentId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual Artist Artist { get; private set; } = null!;
    public virtual User SubmittedByUser { get; private set; } = null!;
    public virtual Album? Album { get; private set; }

    private BannerRequest() { }

    public static BannerRequest Create(
        Guid artistId, 
        Guid userId, 
        Guid? albumId,
        string title, 
        string? bannerUrl, 
        DateTime utcNow)
    {
        return new BannerRequest
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            SubmittedByUserId = userId,
            AlbumId = albumId,
            Title = title.Trim(),
            BannerUrl = bannerUrl,
            Status = BannerRequestStatus.AwaitingPayment, 
            IsPaid = false, 
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public void SetCheckoutSession(string sessionId)
    {
        StripeSessionId = sessionId;
    }

    public void MarkAsPaid(string paymentIntentId, DateTime utcNow)
    {
        IsPaid = true;
        Status = BannerRequestStatus.Pending;
        StripePaymentIntentId = paymentIntentId;
        UpdatedAt = utcNow;
    }

    public void Approve(DateTime utcNow)
    {
        Status = BannerRequestStatus.Approved;
        UpdatedAt = utcNow;
    }

    public void Reject(string? reason, DateTime utcNow)
    {
        Status = BannerRequestStatus.Rejected;
        AdminNotes = reason;
        UpdatedAt = utcNow;
    }
}
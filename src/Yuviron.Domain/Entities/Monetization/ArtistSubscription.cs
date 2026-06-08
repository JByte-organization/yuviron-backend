using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using System;

namespace Yuviron.Domain.Entities;

public class ArtistSubscription : Entity
{
    public Guid ArtistId { get; private set; }
    public Guid PlanId { get; private set; } 
    public Guid PayerUserId { get; private set; } 

    public SubscriptionStatus Status { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public string? StripeSubscriptionId { get; private set; } 
    public bool IsAutoRenewing { get; private set; } = true;

    public virtual Artist Artist { get; private set; } = null!;
    public virtual Plan Plan { get; private set; } = null!;
    public virtual User PayerUser { get; private set; } = null!;

    private ArtistSubscription() { }

    public static ArtistSubscription Create(
        Guid artistId,
        Guid payerUserId,
        Guid planId, 
        DateTime startAt, 
        DateTime endAt, 
        SubscriptionStatus status,
        DateTime utcNow,
        string? stripeSubscriptionId = null)
    {
        return new ArtistSubscription
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            PayerUserId = payerUserId,
            PlanId = planId,
            StartAt = startAt,
            EndAt = endAt,
            Status = status,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            StripeSubscriptionId = stripeSubscriptionId
        };
    }
    
    public void Cancel(DateTime utcNow, bool immediate = false)
    {
        if (Status == SubscriptionStatus.Cancelled) return; 

        Status = SubscriptionStatus.Cancelled;
        UpdatedAt = utcNow;
        
        if (immediate)
        {
            EndAt = utcNow; 
        }
    }

    public void CancelRenewal(DateTime utcNow)
    {
        IsAutoRenewing = false;
        UpdatedAt = utcNow;
    }

    public void MarkAsPastDue(DateTime utcNow)
    {
        if (Status == SubscriptionStatus.Cancelled || Status == SubscriptionStatus.Expired)
        {
            return;
        }

        Status = SubscriptionStatus.PastDue;
        UpdatedAt = utcNow;
    }

    public void Renew(DateTime newEndDate, DateTime utcNow)
    {
        EndAt = newEndDate;
        Status = SubscriptionStatus.Active;
        UpdatedAt = utcNow;
    }
}

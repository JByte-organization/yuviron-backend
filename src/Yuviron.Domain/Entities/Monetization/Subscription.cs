using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using System;

namespace Yuviron.Domain.Entities;

public class Subscription : Entity
{
    public Guid UserId { get; private set; }
    public Guid PlanId { get; private set; }

    public SubscriptionStatus Status { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public string? StripeSubscriptionId { get; private set; }
    public bool IsAutoRenewing { get; private set; } = true;

    public virtual User User { get; private set; } = null!;
    public virtual Plan Plan { get; private set; } = null!;

    private Subscription() { }

    public static Subscription Create(
        Guid userId, 
        Guid planId, 
        DateTime startAt, 
        DateTime endAt, 
        SubscriptionStatus status,
        DateTime utcNow,
        string? stripeSubscriptionId = null)
    {
        return new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
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

    public void Renew(DateTime newEndDate, DateTime utcNow)
    {
        EndAt = newEndDate;
        Status = SubscriptionStatus.Active;
        UpdatedAt = utcNow;
    }
}
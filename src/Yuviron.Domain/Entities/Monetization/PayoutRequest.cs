using System;
using System.Collections.Generic;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums; // <-- Подключили енамку

namespace Yuviron.Domain.Entities;

public sealed class PayoutRequest : Entity // <-- Добавили sealed
{
    public Guid ArtistId { get; private set; }
    public decimal RequestedAmount { get; private set; }
    public PayoutStatus Status { get; private set; }

    public DateTime RequestedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; } 
    
    public Guid? AdminId { get; private set; } 
    public string? DecisionNote { get; private set; }

    public Artist Artist { get; private set; } = null!;
    public User? Admin { get; private set; }

    public ICollection<PayoutTransaction> Transactions { get; private set; } = new List<PayoutTransaction>();

    private PayoutRequest() { }

    public static PayoutRequest Create(Guid artistId, decimal amount, DateTime utcNow)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");

        return new PayoutRequest
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            RequestedAmount = amount,
            Status = PayoutStatus.Pending,
            RequestedAt = utcNow,
            UpdatedAt = utcNow 
        };
    }

    public void Approve(Guid adminId, DateTime utcNow)
    {
        if (Status != PayoutStatus.Pending) throw new InvalidOperationException("Can only approve pending requests.");
        Status = PayoutStatus.Approved;
        AdminId = adminId;
        UpdatedAt = utcNow;
    }

    public void Reject(Guid adminId, string note, DateTime utcNow)
    {
        if (Status != PayoutStatus.Pending) throw new InvalidOperationException("Can only reject pending requests.");
        Status = PayoutStatus.Rejected;
        AdminId = adminId;
        DecisionNote = note?.Trim();
        UpdatedAt = utcNow; 
    }

    public void MarkAsPaid(DateTime utcNow) 
    {
        if (Status != PayoutStatus.Approved) throw new InvalidOperationException("Must be approved before paying.");
        Status = PayoutStatus.Paid;
        UpdatedAt = utcNow; 
    }
}
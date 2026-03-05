using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public enum TransactionStatus { Success = 1, Failed = 2 }

public class PayoutTransaction : Entity
{
    public Guid PayoutRequestId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaidAt { get; private set; }
    public string? ProviderRef { get; private set; } 
    public TransactionStatus Status { get; private set; }

    public virtual PayoutRequest PayoutRequest { get; private set; } = null!;

    private PayoutTransaction() { }

    public static PayoutTransaction Create(Guid payoutReqId, decimal amount, string? providerRef, TransactionStatus status, DateTime utcNow)
    {
        return new PayoutTransaction
        {
            Id = Guid.NewGuid(),
            PayoutRequestId = payoutReqId,
            Amount = amount,
            ProviderRef = providerRef,
            Status = status,
            PaidAt = utcNow
        };
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public enum TransactionStatus { Success = 1, Failed = 2 }

public class PayoutTransaction : Entity
{
    public Guid PayoutRequestId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaidAt { get; private set; }
    public string? ProviderRef { get; private set; } // ID транзакции в PayPal/Stripe
    public TransactionStatus Status { get; private set; }

    public virtual PayoutRequest PayoutRequest { get; private set; } = null!;

    private PayoutTransaction() { }
}

using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public enum PayoutStatus { Pending = 1, Approved = 2, Rejected = 3, Paid = 4 }

public class PayoutRequest : Entity
{
    public Guid ArtistId { get; private set; }
    public decimal RequestedAmount { get; private set; }
    public PayoutStatus Status { get; private set; }

    public DateTime RequestedAt { get; private set; }
    public Guid? AdminId { get; private set; } // Кто обработал
    public string? DecisionNote { get; private set; }

    public virtual Artist Artist { get; private set; } = null!;
    public virtual User? Admin { get; private set; }

    public virtual ICollection<PayoutTransaction> Transactions { get; private set; } = new List<PayoutTransaction>();

    private PayoutRequest() { }
}

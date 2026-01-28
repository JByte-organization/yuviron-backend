using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class Subscription : Entity
{
    public Guid UserId { get; private set; }
    public Guid PlanId { get; private set; }

    public SubscriptionStatus Status { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual Plan Plan { get; private set; } = null!;

    private Subscription() { }
}
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record SubscriptionRenewedEvent(
    Guid UserId,
    Guid PlanId,
    string PlanName,
    DateTime EndAt
) : IDomainEvent;

using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record SubscriptionCanceledEvent(
    Guid UserId,
    string PlanName
) : IDomainEvent;

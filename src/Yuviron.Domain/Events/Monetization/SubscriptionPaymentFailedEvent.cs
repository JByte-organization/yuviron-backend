using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record SubscriptionPaymentFailedEvent(
    Guid UserId,
    Guid PlanId,
    string PlanName,
    string StripeSubscriptionId,
    string? FailureReason
) : IDomainEvent;

using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record ArtistSubscriptionPaymentFailedEvent(
    Guid ArtistId,
    Guid PayerUserId,
    Guid PlanId,
    string PlanName,
    string StripeSubscriptionId,
    string? FailureReason
) : IDomainEvent, IArtistEvent;

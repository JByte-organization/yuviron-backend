using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record ArtistSubscriptionActivatedEvent(
    Guid ArtistId,
    Guid PayerUserId,
    Guid PlanId,
    string PlanName,
    DateTime EndAt
) : IDomainEvent, IArtistEvent;

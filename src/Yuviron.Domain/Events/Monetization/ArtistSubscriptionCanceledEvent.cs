using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record ArtistSubscriptionCanceledEvent(Guid ArtistId) : IDomainEvent, IArtistEvent;
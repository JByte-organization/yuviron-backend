using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record PayoutRequestedEvent(Guid ArtistId, decimal Amount) : IDomainEvent, IArtistEvent;
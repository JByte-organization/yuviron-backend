using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record TrackPlayedFallbackEvent(Guid TrackId, Guid UserId) : IDomainEvent;
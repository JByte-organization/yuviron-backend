using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record TrackDeletedEvent(Guid TrackId) : IDomainEvent;
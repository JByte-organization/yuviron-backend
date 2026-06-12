using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record PlaylistDeletedEvent(Guid PlaylistId) : IDomainEvent;

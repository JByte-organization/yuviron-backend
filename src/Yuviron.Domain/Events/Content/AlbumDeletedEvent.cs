using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record AlbumDeletedEvent(Guid AlbumId) : IDomainEvent;
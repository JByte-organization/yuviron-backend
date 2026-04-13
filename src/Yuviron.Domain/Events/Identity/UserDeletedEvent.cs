using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record UserDeletedEvent(Guid UserId) : IDomainEvent;
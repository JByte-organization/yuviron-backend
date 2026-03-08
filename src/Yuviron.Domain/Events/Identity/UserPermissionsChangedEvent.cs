using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record UserPermissionsChangedEvent(Guid UserId) : IDomainEvent;
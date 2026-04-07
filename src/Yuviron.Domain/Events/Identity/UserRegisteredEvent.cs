using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record UserRegisteredEvent(Guid UserId, string Email, string FirstName) : IDomainEvent;
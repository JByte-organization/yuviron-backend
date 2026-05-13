using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record UserEmailConfirmedEvent(Guid UserId, string Email, string FirstName) : IDomainEvent;
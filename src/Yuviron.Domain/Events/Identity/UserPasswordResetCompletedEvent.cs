using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record UserPasswordResetCompletedEvent(Guid UserId) : IDomainEvent;

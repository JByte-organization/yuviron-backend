using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record ForgotPasswordRequestedEvent(Guid UserId, string Email, string FirstName, string ResetToken) : IDomainEvent;
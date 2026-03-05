using MediatR;

namespace Yuviron.Domain.Events;

public record UserDeletedEvent(Guid UserId) : INotification;
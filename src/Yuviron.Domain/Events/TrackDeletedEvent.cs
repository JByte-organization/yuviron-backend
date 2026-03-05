using MediatR;

namespace Yuviron.Domain.Events;

public record TrackDeletedEvent(Guid TrackId) : INotification;
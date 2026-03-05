using MediatR;

namespace Yuviron.Domain.Events;

public record ArtistDeletedEvent(Guid ArtistId) : INotification;
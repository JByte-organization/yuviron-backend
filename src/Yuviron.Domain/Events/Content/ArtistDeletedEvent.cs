using MediatR;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record ArtistDeletedEvent(Guid ArtistId) : IDomainEvent;
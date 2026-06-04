using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record TeamMemberRemovedEvent(Guid UserId, Guid ArtistId, string ArtistName) : IDomainEvent, IArtistEvent;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record ModeratedTrackDeletedEvent(
    Guid TrackId,
    Guid ArtistId,
    string TrackTitle
) : IDomainEvent, IArtistEvent;

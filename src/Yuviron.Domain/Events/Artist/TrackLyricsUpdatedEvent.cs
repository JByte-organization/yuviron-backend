using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record TrackLyricsUpdatedEvent(
    Guid ArtistId,
    Guid TrackId,
    string TrackTitle,
    Guid UpdatedByUserId
) : IDomainEvent, IArtistEvent;

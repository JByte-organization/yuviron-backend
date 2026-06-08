using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record StudioPlaylistTrackChangedEvent(
    Guid ArtistId,
    Guid PlaylistId,
    string PlaylistName,
    Guid TrackId,
    string TrackTitle,
    Guid ChangedByUserId,
    string Action
) : IDomainEvent, IArtistEvent;

using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record TrackAddedToEditorialPlaylistEvent(Guid ArtistId, Guid TrackId, string TrackTitle, string PlaylistName) : IDomainEvent, IArtistEvent;
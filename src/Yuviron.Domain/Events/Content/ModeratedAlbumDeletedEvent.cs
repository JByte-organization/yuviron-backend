using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record ModeratedAlbumDeletedEvent(
    Guid AlbumId,
    Guid ArtistId,
    string AlbumTitle
) : IDomainEvent, IArtistEvent;

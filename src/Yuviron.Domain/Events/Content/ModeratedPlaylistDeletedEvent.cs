using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record ModeratedPlaylistDeletedEvent(
    Guid PlaylistId,
    Guid? UserId,
    string PlaylistTitle
) : IDomainEvent;

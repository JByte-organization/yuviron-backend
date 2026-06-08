using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record TrackTrendingEvent(
    Guid ArtistId,
    Guid TrackId,
    string TrackTitle,
    long Current24hPlays,
    long Previous24hPlays
) : IDomainEvent, IArtistEvent;

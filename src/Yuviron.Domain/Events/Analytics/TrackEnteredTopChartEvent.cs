using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record TrackEnteredTopChartEvent(
    Guid ArtistId,
    Guid TrackId,
    string TrackTitle,
    int Rank,
    long PlayCount
) : IDomainEvent, IArtistEvent;

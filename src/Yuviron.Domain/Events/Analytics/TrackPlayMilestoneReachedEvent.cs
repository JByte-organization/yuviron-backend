using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record TrackPlayMilestoneReachedEvent(
    Guid ArtistId,
    Guid TrackId,
    string TrackTitle,
    long PlayCount,
    long Milestone
) : IDomainEvent, IArtistEvent;

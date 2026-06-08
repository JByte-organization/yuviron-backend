using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record ArtistFollowersMilestoneReachedEvent(
    Guid ArtistId,
    string ArtistName,
    int FollowerCount,
    int Milestone
) : IDomainEvent, IArtistEvent;

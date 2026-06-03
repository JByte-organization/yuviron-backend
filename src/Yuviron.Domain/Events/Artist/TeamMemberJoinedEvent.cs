
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Events;

public sealed record TeamMemberJoinedEvent(
    Guid ArtistId,
    string ArtistName,
    string JoinedUserEmail,
    ArtistTeamRole Role
) : IDomainEvent;
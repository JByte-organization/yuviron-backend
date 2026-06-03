using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Events;

public sealed record TeamRoleChangedEvent(
    Guid TargetUserId,
    Guid ArtistId,
    string ArtistName,
    ArtistTeamRole NewRole
) : IDomainEvent;
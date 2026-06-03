using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record SendTeamInviteEmailEvent(
    string Email,
    string ArtistName,
    string RoleName,
    string InviteToken
) : IDomainEvent;
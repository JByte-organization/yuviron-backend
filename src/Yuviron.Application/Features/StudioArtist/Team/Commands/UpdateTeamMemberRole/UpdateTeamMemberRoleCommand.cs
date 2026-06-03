using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Team.Commands.UpdateTeamMemberRole;

public sealed record UpdateTeamMemberRoleCommand(
    Guid ArtistId,
    Guid TargetUserId,
    ArtistTeamRole NewRole
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
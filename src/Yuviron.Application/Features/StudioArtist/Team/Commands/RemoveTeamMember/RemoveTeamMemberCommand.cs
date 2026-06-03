using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Team.Commands.RemoveTeamMember;

public sealed record RemoveTeamMemberCommand(
    Guid ArtistId,
    Guid TargetUserId
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
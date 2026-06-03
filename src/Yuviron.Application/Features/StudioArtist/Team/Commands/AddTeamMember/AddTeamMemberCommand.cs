using FluentValidation;
using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Team.Commands.AddTeamMember;

public sealed record AddTeamMemberCommand(
    Guid ArtistId,
    string UserEmail,
    ArtistTeamRole Role
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
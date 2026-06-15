using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Team.Commands.AcceptTeamInvite;

public sealed record AcceptTeamInviteCommand(string Token) : IRequest<Unit>, ISecuredRequest
{
    AppPermission ISecuredRequest.RequiredPermission => AppPermission.AccessBasic; 
}

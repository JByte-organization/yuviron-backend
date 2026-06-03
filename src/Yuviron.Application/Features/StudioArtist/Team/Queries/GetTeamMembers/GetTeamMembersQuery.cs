using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Team.Queries.GetTeamMembers;

public sealed record GetTeamMembersQuery(Guid ArtistId) : IRequest<List<TeamMemberDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}
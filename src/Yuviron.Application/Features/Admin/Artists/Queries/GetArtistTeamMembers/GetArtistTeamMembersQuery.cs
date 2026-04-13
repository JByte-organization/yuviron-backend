using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistTeamMembers;

public sealed record GetArtistTeamMembersQuery(Guid ArtistId) : IRequest<List<ArtistTeamMemberDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
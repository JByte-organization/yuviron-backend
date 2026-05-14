using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistTeamMembers;

public sealed record GetArtistTeamMembersQuery(
    Guid ArtistId,
    string? SearchTerm = null,
    string? SortBy = null,    
    string? SortOrder = null,  
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<ArtistTeamMemberDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylists;

public sealed record GetPlaylistsQuery(
    Guid? TrackId = null,
    string? SearchTerm = null,
    string? SortBy = null,  
    string? SortOrder = null, 
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<PlaylistDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}

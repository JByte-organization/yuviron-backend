using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylists;

public sealed record GetPlaylistsQuery(
    string? SearchTerm,
    bool IncludeDeleted,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, IncludeDeleted, Page, PageSize), 
    IRequest<PaginatedList<PlaylistDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
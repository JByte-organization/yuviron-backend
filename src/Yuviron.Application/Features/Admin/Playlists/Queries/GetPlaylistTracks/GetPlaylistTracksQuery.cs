using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistTracks;

public sealed record GetPlaylistTracksQuery(
    Guid PlaylistId,
    string? SearchTerm = null,
    string? SortBy = null,
    string? SortOrder = null,
    int Page = 1,
    int PageSize = 50
) : PaginatedQuery(SearchTerm, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<PlaylistTrackItemDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
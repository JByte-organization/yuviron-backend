using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetTracks;

public sealed record GetStudioArtistTracksQuery(
    string? SortBy = null,
    string? SortOrder = null,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, SortBy, SortOrder, Page, PageSize),
    IRequest<PaginatedList<StudioArtistTrackListItemDto>>,
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}

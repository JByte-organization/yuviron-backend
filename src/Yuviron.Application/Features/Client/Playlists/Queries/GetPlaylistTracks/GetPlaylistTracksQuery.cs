using MediatR;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistTracks;

public sealed record GetPlaylistTracksQuery(
    Guid PlaylistId,
    string? SortBy = null,
    string? SortOrder = null,
    int Page = 1,
    int PageSize = 50
) : PaginatedQuery(null, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<PlaylistTrackItemClientDto>>;
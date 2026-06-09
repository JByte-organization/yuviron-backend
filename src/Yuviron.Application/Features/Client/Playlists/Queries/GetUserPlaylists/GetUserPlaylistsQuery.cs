using MediatR;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;

namespace Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;

//это левое боковое меню в десктопном Spotify, где списком идут все твои плейлисты.
public sealed record GetUserPlaylistsQuery(
    Guid? TrackId = null,
    string? SortBy = null,    
    string? SortOrder = null,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, SortBy, SortOrder, Page, PageSize),
    IRequest<PaginatedList<UserPlaylistDto>>;

using MediatR;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;

namespace Yuviron.Application.Features.Client.Playlists.Queries.GetDeletedPlaylists;

public sealed record GetDeletedPlaylistsQuery(
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, null, null, Page, PageSize),
    IRequest<PaginatedList<UserPlaylistDto>>;

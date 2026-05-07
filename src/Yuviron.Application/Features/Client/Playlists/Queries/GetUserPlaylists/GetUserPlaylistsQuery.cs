using MediatR;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserPlaylists;

public sealed record GetUserPlaylistsQuery(
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, Page, PageSize),
    IRequest<PaginatedList<UserPlaylistDto>>;

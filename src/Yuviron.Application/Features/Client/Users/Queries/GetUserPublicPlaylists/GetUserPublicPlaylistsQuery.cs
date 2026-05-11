using MediatR;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Library.Queries.GetUserPlaylists;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists; // Переиспользуем DTO

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserPublicPlaylists;

public sealed record GetUserPublicPlaylistsQuery(
    Guid TargetUserId,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, Page, PageSize), IRequest<PaginatedList<UserPlaylistDto>>;
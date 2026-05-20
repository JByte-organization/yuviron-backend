using MediatR;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists; 

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserPublicPlaylists;

//Повертає список плейлістів конкретного користувача.
public sealed record GetUserPublicPlaylistsQuery(
    Guid TargetUserId,
    string? SortBy = null,  
    string? SortOrder = null,  
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<UserPlaylistDto>>;
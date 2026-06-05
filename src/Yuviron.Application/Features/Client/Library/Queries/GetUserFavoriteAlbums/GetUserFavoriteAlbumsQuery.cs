using MediatR;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteAlbums;

public sealed record GetUserFavoriteAlbumsQuery(string? SortBy = null, string? SortOrder = null, int Page = 1, int PageSize = 20) 
    : PaginatedQuery(null, SortBy, SortOrder, Page, PageSize), IRequest<PaginatedList<UserFavoriteAlbumDto>>;
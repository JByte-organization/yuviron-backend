using MediatR;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteArtists;

public sealed record GetFollowedArtistsQuery(
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, Page, PageSize),
    IRequest<PaginatedList<FollowedArtistDto>>;

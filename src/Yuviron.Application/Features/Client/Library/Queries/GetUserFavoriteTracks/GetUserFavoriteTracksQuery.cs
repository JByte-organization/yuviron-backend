using MediatR;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteTracks;

public sealed record GetUserFavoriteTracksQuery(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "savedAt",
    string? SortOrder = "desc"
) : PaginatedQuery(null, Page, PageSize),
    IRequest<PaginatedList<UserFavoriteTrackDto>>;

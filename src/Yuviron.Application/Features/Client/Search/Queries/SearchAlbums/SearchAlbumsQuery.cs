using MediatR;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

namespace Yuviron.Application.Features.Client.Search.Queries.SearchAlbums;

public sealed record SearchAlbumsQuery(
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, null, null, Page, PageSize),
    IRequest<PaginatedList<SearchAlbumDto>>;

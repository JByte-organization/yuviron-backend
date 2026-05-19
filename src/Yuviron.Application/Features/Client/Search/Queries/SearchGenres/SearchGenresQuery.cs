using MediatR;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Search.Queries.SearchGenres;

public sealed record SearchGenresQuery(
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, null, null, Page, PageSize),
    IRequest<PaginatedList<SearchGenreMoodDto>>;

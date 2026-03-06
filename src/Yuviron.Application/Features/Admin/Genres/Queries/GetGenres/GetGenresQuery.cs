using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Genres.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Genres.Queries.GetGenres;

public sealed record GetGenresQuery(
    string? SearchTerm,
    bool IncludeDeleted,
    int Page = 1,
    int PageSize = 50 
) : PaginatedQuery(SearchTerm, IncludeDeleted, Page, PageSize), 
    IRequest<PaginatedList<GenreDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
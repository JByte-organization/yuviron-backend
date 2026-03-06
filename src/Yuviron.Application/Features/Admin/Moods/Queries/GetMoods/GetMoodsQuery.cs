using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Moods.Queries.GetMoods;

public sealed record GetMoodsQuery(
    string? SearchTerm,
    bool IncludeDeleted,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, IncludeDeleted, Page, PageSize), 
    IRequest<PaginatedList<MoodDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
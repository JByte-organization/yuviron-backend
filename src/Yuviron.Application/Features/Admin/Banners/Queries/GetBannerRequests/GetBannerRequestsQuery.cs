using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBannerRequests;

public sealed record GetBannerRequestsQuery(
    BannerRequestStatus? Status = null, 
    string? SearchTerm = null,
    string? SortBy = null,
    string? SortOrder = null,
    int Page = 1,
    int PageSize = 50
) : PaginatedQuery(SearchTerm, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<BannerRequestListItemDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Finance.Queries.GetPayoutRequests;

public sealed record GetPayoutRequestsQuery(
    PayoutStatus? Status,
    string? SearchTerm = null,
    string? SortBy = null,
    string? SortOrder = null,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, SortBy, SortOrder, Page, PageSize),
    IRequest<PaginatedList<PayoutRequestListItemDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel; 
}
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.VerificationRequests.Queries.GetRequests;

public sealed record GetVerificationRequestsQuery(
    VerificationRequestStatus? Status = VerificationRequestStatus.Pending,
    string? SearchTerm = null,
    string? SortBy = null,   
    string? SortOrder = null, 
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<VerificationRequestListItemDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}
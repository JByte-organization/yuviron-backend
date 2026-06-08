using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Complaints.Queries.GetComplaints;

public sealed record GetComplaintsQuery(
    ComplaintStatus? Status = null,
    ComplaintTargetType? TargetType = null,
    string? SearchTerm = null,
    string? SortBy = null,   
    string? SortOrder = null, 
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<ComplaintListItemDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}
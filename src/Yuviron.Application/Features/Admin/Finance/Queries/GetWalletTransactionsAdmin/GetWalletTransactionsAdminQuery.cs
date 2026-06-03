using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Finance.Queries.GetWalletTransactionsAdmin;

public sealed record GetWalletTransactionsAdminQuery(
    Guid ArtistId,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, null, null, Page, PageSize), 
    IRequest<PaginatedList<AdminWalletTransactionDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Finance.Queries.GetWalletTransactions;

public sealed record GetWalletTransactionsQuery(
    Guid ArtistId,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, null, null, Page, PageSize), 
    IRequest<PaginatedList<WalletTransactionDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
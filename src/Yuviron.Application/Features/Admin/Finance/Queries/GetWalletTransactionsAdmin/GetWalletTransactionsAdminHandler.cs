using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Finance.Queries.GetWalletTransactionsAdmin;

public sealed class GetWalletTransactionsAdminHandler : IRequestHandler<GetWalletTransactionsAdminQuery, PaginatedList<AdminWalletTransactionDto>>
{
    private readonly IMonetizationContext _monetizationContext;

    public GetWalletTransactionsAdminHandler(IMonetizationContext monetizationContext)
    {
        _monetizationContext = monetizationContext;
    }

    public async Task<PaginatedList<AdminWalletTransactionDto>> Handle(GetWalletTransactionsAdminQuery request, CancellationToken cancellationToken)
    {
        var query = _monetizationContext.WalletTransactions
            .AsNoTracking()
            .Where(wt => wt.Wallet.ArtistId == request.ArtistId);

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(WalletTransaction.CreatedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<WalletTransaction, object>>>());

        var projectedQuery = sortedQuery.Select(wt => new AdminWalletTransactionDto(
            wt.Id, wt.Amount, wt.Type, wt.Description, wt.CreatedAt));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
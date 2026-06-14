using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Finance.Queries.GetWalletTransactions;

public sealed class GetWalletTransactionsHandler : IRequestHandler<GetWalletTransactionsQuery, PaginatedList<WalletTransactionDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly IMonetizationContext _monetizationContext;
    private readonly ICurrentUserService _currentUser;

    public GetWalletTransactionsHandler(ICatalogContext catalogContext, IMonetizationContext monetizationContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _monetizationContext = monetizationContext; _currentUser = currentUser;
    }

    public async Task<PaginatedList<WalletTransactionDto>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == userId, cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this artist's finances.");

        var query = _monetizationContext.WalletTransactions
            .AsNoTracking()
            .Where(wt => wt.Wallet.ArtistId == request.ArtistId);

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(WalletTransaction.CreatedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<WalletTransaction, object>>>
            {
                [nameof(WalletTransactionDto.Amount)] = wt => wt.Amount
            });

        var projectedQuery = sortedQuery.Select(wt => new WalletTransactionDto(
            wt.Id, wt.Amount, wt.Type, wt.Description, wt.CreatedAt));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
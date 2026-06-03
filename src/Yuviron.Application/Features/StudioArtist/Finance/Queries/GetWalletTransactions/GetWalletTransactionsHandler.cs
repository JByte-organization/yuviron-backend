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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetWalletTransactionsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context; _currentUser = currentUser;
    }

    public async Task<PaginatedList<WalletTransactionDto>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasAccess = await _context.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == userId, cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this artist's finances.");

        var query = _context.WalletTransactions
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
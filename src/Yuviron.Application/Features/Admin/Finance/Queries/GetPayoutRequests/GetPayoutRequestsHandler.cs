using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Finance.Queries.GetPayoutRequests;

public sealed class GetPayoutRequestsHandler : IRequestHandler<GetPayoutRequestsQuery, PaginatedList<PayoutRequestListItemDto>>
{
    private readonly IMonetizationContext _monetizationContext;

    public GetPayoutRequestsHandler(IMonetizationContext monetizationContext)
    {
        _monetizationContext = monetizationContext;
    }

    public async Task<PaginatedList<PayoutRequestListItemDto>> Handle(GetPayoutRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _monetizationContext.PayoutRequests
            .Include(pr => pr.Artist)
                .ThenInclude(a => a.ArtistWallet) 
            .Include(pr => pr.Admin)
                .ThenInclude(u => u!.Profile) 
            .AsNoTracking();

        if (request.Status.HasValue)
            query = query.Where(pr => pr.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(pr => pr.Artist.Name.Contains(request.SearchTerm));

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(PayoutRequest.RequestedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<PayoutRequest, object>>>
            {
                ["ArtistName"] = pr => pr.Artist.Name,
                ["Amount"] = pr => pr.RequestedAmount,
                ["RequestedAt"] = pr => pr.RequestedAt
            });

        var projectedQuery = sortedQuery.Select(pr => new PayoutRequestListItemDto(
            pr.Id,
            new SimpleArtistDto(pr.ArtistId, pr.Artist.Name),
            pr.RequestedAmount,
            pr.Artist.ArtistWallet != null ? pr.Artist.ArtistWallet.TotalEarned : 0m, 
            pr.Status,
            pr.RequestedAt,
            pr.UpdatedAt,
            pr.Admin != null && pr.Admin.Profile != null 
                ? pr.Admin.Profile.FirstName 
                : null,
            pr.DecisionNote
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.StudioArtist.Finance.Queries.GetArtistPayoutRequests;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

public sealed class GetArtistPayoutRequestsHandler : IRequestHandler<GetArtistPayoutRequestsQuery, PaginatedList<ArtistPayoutRequestDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly IMonetizationContext _monetizationContext;
    private readonly ICurrentUserService _currentUser;

    public GetArtistPayoutRequestsHandler(ICatalogContext catalogContext, IMonetizationContext monetizationContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _monetizationContext = monetizationContext;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ArtistPayoutRequestDto>> Handle(GetArtistPayoutRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == userId, cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this artist's finances.");

        var query = _monetizationContext.PayoutRequests
            .AsNoTracking()
            .Where(pr => pr.ArtistId == request.ArtistId);

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(PayoutRequest.RequestedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<PayoutRequest, object>>>());

        var projectedQuery = sortedQuery.Select(pr => new ArtistPayoutRequestDto(
            pr.Id,
            pr.RequestedAmount,
            pr.Status,
            pr.RequestedAt,
            pr.DecisionNote
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
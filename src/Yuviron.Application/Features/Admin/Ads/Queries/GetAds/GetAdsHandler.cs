using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Admin.Ads.Queries.GetAds;

public sealed class GetAdsHandler : IRequestHandler<GetAdsQuery, PaginatedList<AdSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAdsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AdSummaryDto>> Handle(GetAdsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Ads
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AdSummaryDto(
                a.Id,
                a.AdvertiserName,
                a.Title,
                a.IsActive,
                a.Impressions.Count, 
                a.Impressions.Count(i => i.IsClicked), 
                a.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<AdSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
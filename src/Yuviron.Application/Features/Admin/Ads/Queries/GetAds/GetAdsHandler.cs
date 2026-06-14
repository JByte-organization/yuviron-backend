using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Ads.Queries.GetAds;

public sealed class GetAdsHandler : IRequestHandler<GetAdsQuery, PaginatedList<AdSummaryDto>>
{
    private readonly IMonetizationContext _monetizationContext;

    public GetAdsHandler(IMonetizationContext monetizationContext) => _monetizationContext = monetizationContext;

    public async Task<PaginatedList<AdSummaryDto>> Handle(GetAdsQuery request, CancellationToken cancellationToken)
    {
        var query = _monetizationContext.Ads.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(a => a.AdvertiserName.StartsWith(request.SearchTerm) || a.Title.StartsWith(request.SearchTerm));
        }

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder,
            defaultSortBy: "CreatedAt",
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<Ad, object>>>
            {
                [nameof(AdSummaryDto.ImpressionsCount)] = a => a.Impressions.Count,
                [nameof(AdSummaryDto.ClicksCount)] = a => a.Impressions.Count(i => i.IsClicked)
            });

        var projectedQuery = sortedQuery.Select(a => new AdSummaryDto(
            a.Id,
            a.AdvertiserName,
            a.ImageUrl,
            a.Title,
            a.IsActive,
            a.Impressions.Count, 
            a.Impressions.Count(i => i.IsClicked), 
            a.CreatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
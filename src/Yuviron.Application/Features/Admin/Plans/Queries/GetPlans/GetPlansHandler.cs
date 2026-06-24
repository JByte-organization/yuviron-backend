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
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Plans.Queries.GetPlans;

public sealed class GetPlansHandler : IRequestHandler<GetPlansQuery, PaginatedList<PlanListItemDto>>
{
    private readonly IMonetizationContext _monetizationContext;

    public GetPlansHandler(IMonetizationContext monetizationContext) => _monetizationContext = monetizationContext;

    public async Task<PaginatedList<PlanListItemDto>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var query = _monetizationContext.Plans.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => p.Name.StartsWith(request.SearchTerm));
        }

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder,
            defaultSortBy: "Price",
            defaultDesc: false,
            mapping: new Dictionary<string, Expression<Func<Plan, object>>>
            {
                [nameof(PlanListItemDto.ActiveSubscribersCount)] = p => _monetizationContext.Subscriptions.Count(s => s.PlanId == p.Id && s.Status == SubscriptionStatus.Active)
            });

        var projectedQuery = sortedQuery.Select(p => new PlanListItemDto(
            p.Id, 
            p.Name,
            p.Price,
            p.Currency,
            p.Period,
            p.Type == PlanType.Listener ? nameof(PlanType.Listener) :
            p.Type == PlanType.Artist ? nameof(PlanType.Artist) :
            nameof(PlanType.Unknown),
            _monetizationContext.Subscriptions.Count(s => s.PlanId == p.Id && s.Status == SubscriptionStatus.Active),
            p.CreatedAt,
            p.UpdatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}

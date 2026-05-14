using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions; 
using Yuviron.Application.Features.Admin.Banners.Queries.DTOs;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBanners;

public sealed class GetBannersHandler : IRequestHandler<GetBannersQuery, PaginatedList<BannerListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBannersHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<BannerListItemDto>> Handle(GetBannersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Banners.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(b => b.Title.StartsWith(request.SearchTerm));

        var projectedQuery = query
            .Select(b => new BannerListItemDto(
                b.Id,
                b.Title,
                b.BannerUrl,
                b.TargetUrl,
                b.SortOrder,
                b.IsActive,
                b.CreatedAt,
                b.UpdatedAt
            ));

        
        var sortedQuery = projectedQuery.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(BannerListItemDto.SortOrder), 
            defaultDesc: false);                               

        return await sortedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
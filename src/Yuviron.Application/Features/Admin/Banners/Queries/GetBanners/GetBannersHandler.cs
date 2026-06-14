using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBanners;

public sealed class GetBannersHandler : IRequestHandler<GetBannersQuery, PaginatedList<BannerListItemDto>>
{
    private readonly IContentContext _contentContext;

    public GetBannersHandler(IContentContext contentContext)
    {
        _contentContext = contentContext;
    }

    public async Task<PaginatedList<BannerListItemDto>> Handle(GetBannersQuery request, CancellationToken cancellationToken)
    {
        var query = _contentContext.Banners.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(b => b.Title.Contains(request.SearchTerm));
        }

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(Banner.CreatedAt),
            defaultDesc: true);

        return await sortedQuery
            .Select(b => new BannerListItemDto(
                b.Id,
                b.Title,
                b.BannerUrl,
                b.IsActive,
                b.StartsAtUtc,
                b.EndsAtUtc
            ))
            .ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}

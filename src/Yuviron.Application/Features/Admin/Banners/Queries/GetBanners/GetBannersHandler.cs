using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBanners;

public sealed class GetBannersHandler : IRequestHandler<GetBannersQuery, PaginatedList<BannerListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBannersHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<BannerListItemDto>> Handle(GetBannersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Banners
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
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

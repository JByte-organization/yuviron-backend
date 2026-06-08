using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Client.Home.Queries.GetHomeBanners;

public sealed class GetHomeBannersHandler : IRequestHandler<GetHomeBannersQuery, List<HomeBannerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetHomeBannersHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<List<HomeBannerDto>> Handle(GetHomeBannersQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        return await _context.Banners
            .AsNoTracking()
            .Where(b => b.IsActive && (!b.StartsAtUtc.HasValue || b.StartsAtUtc <= utcNow))
            .OrderBy(b => b.SortOrder)
            .ThenByDescending(b => b.CreatedAt)
            .Take(request.Limit)
            .Select(b => new HomeBannerDto(
                b.Id,
                b.Title,
                b.BannerUrl,
                b.TargetUrl
            ))
            .ToListAsync(cancellationToken);
    }
}

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

        var pool = await _context.Banners
            .AsNoTracking()
            .Where(b => b.IsActive && 
                        (b.StartsAtUtc == null || b.StartsAtUtc <= utcNow) &&
                        (b.EndsAtUtc == null || b.EndsAtUtc >= utcNow))
            .ToListAsync(cancellationToken);

        if (!pool.Any()) return new List<HomeBannerDto>();

        var filteredPool = pool.Where(b =>
        {
            var countryMatch = true;
            if (!string.IsNullOrEmpty(b.TargetCountries))
            {
                if (string.IsNullOrEmpty(request.CountryCode)) 
                {
                    countryMatch = false; 
                }
                else 
                {
                    var countries = b.TargetCountries.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    countryMatch = countries.Contains(request.CountryCode, StringComparer.OrdinalIgnoreCase);
                }
            }

            var genreMatch = true;
            if (!string.IsNullOrEmpty(b.TargetGenres))
            {
                if (request.FavoriteGenreIds == null || !request.FavoriteGenreIds.Any())
                {
                    genreMatch = false;
                }
                else 
                {
                    var targetGenreIds = b.TargetGenres.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var userGenreIds = request.FavoriteGenreIds.Select(g => g.ToString()).ToList();
                    genreMatch = targetGenreIds.Any(tg => userGenreIds.Contains(tg));
                }
            }

            return countryMatch && genreMatch;
        }).ToList();

        var source = filteredPool.Any() ? filteredPool : pool.Where(b => string.IsNullOrEmpty(b.TargetCountries) && string.IsNullOrEmpty(b.TargetGenres)).ToList();
        
        if (!source.Any()) source = pool; 

        var random = new Random();
        var results = source
            .OrderBy(x => random.Next())
            .Take(request.Limit)
            .Select(b => new HomeBannerDto(
                b.Id,
                b.Title,
                b.BannerUrl,
                b.TargetUrl
            ))
            .ToList();

        return results;
    }
}

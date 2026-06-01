using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Features.Client.Home.Queries.GetUserTopArtists;

namespace Yuviron.Application.Features.Client.Home.Queries.GetSystemTopArtists;

public sealed class GetSystemTopArtistsHandler : IRequestHandler<GetSystemTopArtistsQuery, List<TopArtistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetSystemTopArtistsHandler(
        IApplicationDbContext context, 
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<List<TopArtistDto>> Handle(GetSystemTopArtistsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"system_top_artists_limit_{request.Limit}";
        
        var cachedArtists = await _cacheService.GetAsync<List<TopArtistDto>>(cacheKey, cancellationToken);
        if (cachedArtists != null)
        {
            return cachedArtists;
        }

        var dbArtists = await _context.Artists
            .AsNoTracking()
            .OrderByDescending(a => a.MonthlyListenersCount) 
            .Take(request.Limit)
            .Select(a => new TopArtistDto(
                a.Id,
                a.Name,
                a.AvatarUrl,
                _context.UserFollowArtists.Count(ufa => ufa.ArtistId == a.Id) 
            ))
            .ToListAsync(cancellationToken);

        if (dbArtists.Any())
        {
            await _cacheService.SetAsync(cacheKey, dbArtists, TimeSpan.FromDays(1), cancellationToken);
        }

        return dbArtists;
    }
}
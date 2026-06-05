using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Home.Queries.GetUserTopArtists;

namespace Yuviron.Application.Features.Client.Home.Queries.GetSystemTopArtists;

public sealed class GetSystemTopArtistsHandler : IRequestHandler<GetSystemTopArtistsQuery, List<TopArtistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUser;

    public GetSystemTopArtistsHandler(
        IApplicationDbContext context, 
        ICacheService cacheService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _cacheService = cacheService;
        _currentUser = currentUser;
    }

    public async Task<List<TopArtistDto>> Handle(GetSystemTopArtistsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"system_top_artists_limit_{request.Limit}";
        
        var cachedArtists = await _cacheService.GetAsync<List<TopArtistDto>>(cacheKey, cancellationToken);
        
        if (cachedArtists == null)
        {
            cachedArtists = await _context.Artists
                .AsNoTracking()
                .OrderByDescending(a => a.MonthlyListenersCount) 
                .Take(request.Limit)
                .Select(a => new TopArtistDto(
                    a.Id,
                    a.Name,
                    a.AvatarUrl,
                    _context.UserFollowArtists.Count(ufa => ufa.ArtistId == a.Id),
                    false 
                ))
                .ToListAsync(cancellationToken);

            if (cachedArtists.Any())
            {
                await _cacheService.SetAsync(cacheKey, cachedArtists, TimeSpan.FromDays(1), cancellationToken);
            }
        }

        return await cachedArtists.EnrichWithCacheAsync(
            _cacheService, 
            _currentUser.UserId, 
            "followed_artists", 
            x => x.Id, 
            (x, followed) => x with { IsFollowed = followed }, 
            cancellationToken);
    }
}
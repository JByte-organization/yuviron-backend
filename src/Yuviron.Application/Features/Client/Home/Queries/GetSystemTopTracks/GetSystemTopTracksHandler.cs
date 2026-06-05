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
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Home.Queries.GetUserTopTracks;

namespace Yuviron.Application.Features.Client.Home.Queries.GetSystemTopTracks;

public sealed class GetSystemTopTracksHandler : IRequestHandler<GetSystemTopTracksQuery, List<TopTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public GetSystemTopTracksHandler(
        IApplicationDbContext context, 
        ICacheService cacheService,
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _cacheService = cacheService;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<List<TopTrackDto>> Handle(GetSystemTopTracksQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"system_top_tracks_limit_{request.Limit}";
        
        var cachedTracks = await _cacheService.GetAsync<List<TopTrackDto>>(cacheKey, cancellationToken);
        
        if (cachedTracks == null)
        {
            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

            cachedTracks = await _context.Tracks
                .AsNoTracking()
                .AvailableForPublic(utcNow)
                .OrderByDescending(t => t.PlayCount) 
                .Take(request.Limit)
                .Select(t => new TopTrackDto(
                    t.Id,
                    t.Title,
                    t.TrackArtists.Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role)),
                    t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                    false 
                ))
                .ToListAsync(cancellationToken);

            if (cachedTracks.Any())
            {
                await _cacheService.SetAsync(cacheKey, cachedTracks, TimeSpan.FromDays(1), cancellationToken);
            }
        }

        return await cachedTracks.EnrichWithCacheAsync(
            _cacheService, 
            _currentUser.UserId, 
            "saved_tracks", 
            x => x.Id, 
            (x, saved) => x with { IsSaved = saved }, 
            cancellationToken);
    }
}
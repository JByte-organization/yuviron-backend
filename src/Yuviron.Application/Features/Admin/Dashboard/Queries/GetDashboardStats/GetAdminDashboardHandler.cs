using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
// Yuviron.Application.Features.Admin.Dashboard.Queries.GetDashboardStats.GetAdminDashboardHandler
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Dashboard.Queries.GetDashboardStats;

public sealed class GetAdminDashboardHandler : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cacheService;

    public GetAdminDashboardHandler(
        IIdentityContext identityContext, ICatalogContext catalogContext, 
        TimeProvider timeProvider,
        ICacheService cacheService)
    {
        _identityContext = identityContext;
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
        _cacheService = cacheService;
    }

    public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = "admin:dashboard:stats";
        var cachedStats = await _cacheService.GetAsync<AdminDashboardDto>(cacheKey, cancellationToken);
        if (cachedStats != null) return cachedStats;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var dayAgo = utcNow.AddDays(-1);

        // 1. Summary
        var totalUsers = await _identityContext.Users.CountAsync(cancellationToken);
        var newUsers24h = await _identityContext.Users.CountAsync(u => u.CreatedAt >= dayAgo, cancellationToken);
        var premiumUsers = await _identityContext.Users.CountAsync(
            u => u.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active && s.EndAt > utcNow),
            cancellationToken);
        
        var totalTracks = await _catalogContext.Tracks.CountAsync(cancellationToken);
        var totalAlbums = await _catalogContext.Albums.CountAsync(cancellationToken);
        var totalArtists = await _catalogContext.Artists.CountAsync(cancellationToken);
        
        var totalPlays = await _catalogContext.Tracks.SumAsync(t => (long)t.PlayCount, cancellationToken);

        var summary = new DashboardSummaryDto(
            totalTracks, totalArtists, totalAlbums, 
            totalUsers, newUsers24h, premiumUsers, totalPlays
        );

        // 2. Recent users
        var recentUsers = await _identityContext.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new RecentUserDto(
                u.Id, 
                u.Email,
                u.Profile!.FirstName, 
                u.Profile.AvatarUrl,  
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        // 3. Top Genres
        var topGenres = await _catalogContext.Genres
            .AsNoTracking()
            .Select(g => new {
                Genre = g,
                TotalPlays = g.TrackGenres.Sum(tg => tg.Track.PlayCount) 
            })
            .OrderByDescending(x => x.TotalPlays)
            .Take(5)
            .Select(x => new TopEntityDto(
                x.Genre.Id, x.Genre.Name, x.Genre.CoverUrl, x.TotalPlays
            ))
            .ToListAsync(cancellationToken);

        // 4. Top Moods
        var topMoods = await _catalogContext.Moods
            .AsNoTracking()
            .Select(m => new {
                Mood = m,
                TotalPlays = m.TrackMoods.Sum(tm => tm.Track.PlayCount) 
            })
            .OrderByDescending(x => x.TotalPlays)
            .Take(5)
            .Select(x => new TopEntityDto(
                x.Mood.Id, x.Mood.Name, x.Mood.CoverUrl, x.TotalPlays
            ))
            .ToListAsync(cancellationToken);

        // 5. Popular Albums
        var popularAlbums = await _catalogContext.Albums
            .AsNoTracking()
            .Select(a => new {
                Album = a,
                TotalPlays = a.Tracks.Sum(t => t.PlayCount) 
            })
            .OrderByDescending(x => x.TotalPlays)
            .Take(5)
            .Select(x => new PopularAlbumDto(
                x.Album.Id, x.Album.Title, x.Album.CoverUrl, x.TotalPlays
            ))
            .ToListAsync(cancellationToken);

        // 6. Top Artists 
        var topArtists = await _catalogContext.Artists
            .AsNoTracking()
            .Select(a => new {
                Artist = a,
                TotalPlays = a.TrackArtists.Sum(ta => ta.Track.PlayCount)
            })
            .OrderByDescending(x => x.TotalPlays)
            .Take(5)
            .Select(x => new TopEntityDto(
                x.Artist.Id, x.Artist.Name, x.Artist.AvatarUrl, x.TotalPlays
            ))
            .ToListAsync(cancellationToken);

        var result = new AdminDashboardDto(summary, recentUsers, topGenres, topMoods, popularAlbums, topArtists);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

        return result;
    }
}
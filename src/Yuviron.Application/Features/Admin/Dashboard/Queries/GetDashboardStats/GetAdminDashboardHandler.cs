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
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cacheService;

    public GetAdminDashboardHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICacheService cacheService)
    {
        _context = context;
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
        var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted, cancellationToken);
        var newUsers24h = await _context.Users.CountAsync(u => !u.IsDeleted && u.CreatedAt >= dayAgo, cancellationToken);
        var premiumUsers = await _context.Users.CountAsync(
            u => !u.IsDeleted && u.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active && s.EndAt > utcNow),
            cancellationToken);
        
        var totalTracks = await _context.Tracks.CountAsync(t => !t.IsDeleted, cancellationToken);
        var totalAlbums = await _context.Albums.CountAsync(a => !a.IsDeleted, cancellationToken);
        var totalArtists = await _context.Artists.CountAsync(a => !a.IsDeleted, cancellationToken);
        
        var totalPlays = await _context.Tracks
            .Where(t => !t.IsDeleted)
            .SumAsync(t => (long)t.PlayCount, cancellationToken);

        var summary = new DashboardSummaryDto(
            totalTracks, totalArtists, totalAlbums, 
            totalUsers, newUsers24h, premiumUsers, totalPlays
        );

        // 2. Recent users
        var recentUsers = await _context.Users
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
        var topGenres = await _context.Genres
            .AsNoTracking()
            .Select(g => new {
                Genre = g,
                TotalPlays = g.TrackGenres.Where(tg => !tg.Track.IsDeleted).Sum(tg => tg.Track.PlayCount) 
            })
            .OrderByDescending(x => x.TotalPlays)
            .Take(5)
            .Select(x => new TopEntityDto(
                x.Genre.Id, x.Genre.Name, x.Genre.CoverUrl, x.TotalPlays
            ))
            .ToListAsync(cancellationToken);

        // 4. Top Moods
        var topMoods = await _context.Moods
            .AsNoTracking()
            .Select(m => new {
                Mood = m,
                TotalPlays = m.TrackMoods.Where(tm => !tm.Track.IsDeleted).Sum(tm => tm.Track.PlayCount) 
            })
            .OrderByDescending(x => x.TotalPlays)
            .Take(5)
            .Select(x => new TopEntityDto(
                x.Mood.Id, x.Mood.Name, x.Mood.CoverUrl, x.TotalPlays
            ))
            .ToListAsync(cancellationToken);

        // 5. Popular Albums
        var popularAlbums = await _context.Albums
            .AsNoTracking()
            .Select(a => new {
                Album = a,
                TotalPlays = a.Tracks.Where(t => !t.IsDeleted).Sum(t => t.PlayCount) 
            })
            .OrderByDescending(x => x.TotalPlays)
            .Take(5)
            .Select(x => new PopularAlbumDto(
                x.Album.Id, x.Album.Title, x.Album.CoverUrl, x.TotalPlays
            ))
            .ToListAsync(cancellationToken);

        // 6. Top Artists 
        var topArtists = await _context.Artists
            .AsNoTracking()
            .Select(a => new {
                Artist = a,
                TotalPlays = a.TrackArtists.Where(ta => !ta.Track.IsDeleted).Sum(ta => ta.Track.PlayCount)
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

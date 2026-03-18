using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var newUsers24h = await _context.Users.CountAsync(u => u.CreatedAt >= dayAgo, cancellationToken);
        var premiumUsers = await _context.Users.CountAsync(u => u.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active && s.EndAt > utcNow), cancellationToken);
        
        var totalTracks = await _context.Tracks.CountAsync(cancellationToken);
        var totalAlbums = await _context.Albums.CountAsync(cancellationToken);
        var totalArtists = await _context.Artists.CountAsync(cancellationToken);
        
        var totalPlays = await _context.Tracks.SumAsync(t => (long)t.PlayCount, cancellationToken);

        var summary = new DashboardSummaryDto(
            totalTracks, totalArtists, totalAlbums, 
            totalUsers, newUsers24h, premiumUsers, totalPlays
        );

        // 2. Последние пользователи
        var recentUsers = await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new RecentUserDto(
                u.Id, 
                u.Email,
                u.Profile != null ? u.Profile.DisplayName : null,
                u.Profile != null ? u.Profile.AvatarUrl : null,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        // 3. Топ Жанров
        var topGenres = await _context.Genres
            .AsNoTracking()
            .OrderByDescending(g => g.TrackGenres.Sum(tg => tg.Track.PlayCount))
            .Take(5)
            .Select(g => new TopEntityDto(
                g.Id, 
                g.Name, 
                g.CoverUrl,
                g.TrackGenres.Sum(tg => tg.Track.PlayCount) 
            ))
            .ToListAsync(cancellationToken);

        // 4. Топ Настроений
        var topMoods = await _context.Moods
            .AsNoTracking()
            .OrderByDescending(m => m.TrackMoods.Sum(tm => tm.Track.PlayCount))
            .Take(5)
            .Select(m => new TopEntityDto(
                m.Id, 
                m.Name, 
                m.CoverUrl,
                m.TrackMoods.Sum(tm => tm.Track.PlayCount)
            ))
            .ToListAsync(cancellationToken);

        // 5. Популярные Альбомы (сортировка перед проекцией)
        var popularAlbums = await _context.Albums
            .AsNoTracking()
            .OrderByDescending(a => a.Tracks.Sum(t => t.PlayCount))
            .Take(5)
            .Select(a => new PopularAlbumDto(
                a.Id, 
                a.Title, 
                a.CoverUrl, 
                a.Tracks.Sum(t => t.PlayCount)
            ))
            .ToListAsync(cancellationToken);

        var result = new AdminDashboardDto(summary, recentUsers, topGenres, topMoods, popularAlbums);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

        return result;
    }
}
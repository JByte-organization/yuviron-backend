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
        if (cachedStats != null)
        {
            return cachedStats;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var dayAgo = utcNow.AddDays(-1);

        // 1. Статистика по юзерам (Всего, Новые за 24ч, Активные Премиумы)
        var userStats = await _context.Users
            .AsNoTracking()
            .GroupBy(u => 1) 
            .Select(g => new 
            {
                Total = g.Count(),
                New24h = g.Count(u => u.CreatedAt >= dayAgo),
                // Ищем юзеров, у которых есть хотя бы одна активная подписка, и её срок еще не истек
                Premium = g.Count(u => u.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active && s.EndAt > utcNow))
            })
            .FirstOrDefaultAsync(cancellationToken);

        // 2. Общее количество контента
        var totalTracks = await _context.Tracks.CountAsync(cancellationToken);
        var totalAlbums = await _context.Albums.CountAsync(cancellationToken);
        var totalArtists = await _context.Artists.CountAsync(cancellationToken);

        var summary = new DashboardSummaryDto(
            totalTracks,
            totalArtists,
            totalAlbums,
            userStats?.Total ?? 0,
            userStats?.New24h ?? 0,
            userStats?.Premium ?? 0 // <-- Вот наш премиум!
        );

        // 3. Последние 5 зарегистрированных пользователей
        var recentUsers = await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new RecentUserDto(
                u.Id, 
                u.Email, 
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        // 4. Топ 5 Жанров (считаем сумму PlayCount у всех треков в жанре)
        var topGenres = await _context.Genres
            .AsNoTracking()
            .Select(g => new TopEntityDto(
                g.Id, 
                g.Name, 
                g.TrackGenres.Sum(tg => tg.Track.PlayCount) 
            ))
            .OrderByDescending(g => g.TotalPlays)
            .Take(5)
            .ToListAsync(cancellationToken);

        // 5. Топ 5 Настроений (считаем сумму PlayCount у всех треков в настроении)
        var topMoods = await _context.Moods
            .AsNoTracking()
            .Select(m => new TopEntityDto(
                m.Id, 
                m.Name, 
                m.TrackMoods.Sum(tm => tm.Track.PlayCount)
            ))
            .OrderByDescending(m => m.TotalPlays)
            .Take(5)
            .ToListAsync(cancellationToken);

        // 6. Популярные альбомы (считаем сумму PlayCount всех треков в альбоме)
        var popularAlbums = await _context.Albums
            .AsNoTracking()
            .Select(a => new PopularAlbumDto(
                a.Id, 
                a.Title, 
                a.CoverUrl != null ? $"/storage/{a.CoverUrl}" : null, 
                a.Tracks.Sum(t => t.PlayCount)
            ))
            .OrderByDescending(a => a.TotalPlays)
            .Take(5)
            .ToListAsync(cancellationToken);

        // 7. Собираем всё в итоговый результат
        var result = new AdminDashboardDto(
            summary, 
            recentUsers, 
            topGenres, 
            topMoods, 
            popularAlbums);

        // Кешируем на 5 минут
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

        return result;
    }
}
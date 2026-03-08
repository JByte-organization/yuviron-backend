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

        var userStats = await _context.Users
            .GroupBy(u => 1) 
            .Select(g => new 
            {
                Total = g.Count(),
                New24h = g.Count(u => u.CreatedAt >= dayAgo)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var artistStats = await _context.Artists
            .GroupBy(a => 1)
            .Select(g => new 
            {
                Total = g.Count(),
                Pending = g.Count(a => a.VerificationStatus == VerificationStatus.Pending)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalTracks = await _context.Tracks.CountAsync(cancellationToken);
        var totalAlbums = await _context.Albums.CountAsync(cancellationToken);

        var summary = new DashboardSummaryDto(
            totalTracks,
            artistStats?.Total ?? 0,
            totalAlbums,
            userStats?.Total ?? 0,
            userStats?.New24h ?? 0,
            artistStats?.Pending ?? 0
        );

        var recentTracks = await _context.Tracks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new RecentActivityDto(t.Id, t.Title, t.CoverUrl, t.CreatedAt)) 
            .ToListAsync(cancellationToken);

        var pendingArtists = await _context.Artists
            .AsNoTracking()
            .Where(a => a.VerificationStatus == VerificationStatus.Pending)
            .OrderBy(a => a.CreatedAt)
            .Take(5)
            .Select(a => new PendingVerificationDto(a.Id, a.Name, a.AvatarUrl, a.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new AdminDashboardDto(summary, recentTracks, pendingArtists);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

        return result;
    }
}
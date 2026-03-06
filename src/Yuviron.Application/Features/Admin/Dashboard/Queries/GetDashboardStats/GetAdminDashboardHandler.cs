using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Dashboard.Queries.GetDashboardStats;

public sealed class GetAdminDashboardHandler : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetAdminDashboardHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var dayAgo = utcNow.AddDays(-1);

        var totalTracks = await _context.Tracks.CountAsync(cancellationToken);
        var totalArtists = await _context.Artists.CountAsync(cancellationToken);
        var totalAlbums = await _context.Albums.CountAsync(cancellationToken);
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        
        var newUsers24h = await _context.Users
            .CountAsync(u => u.CreatedAt >= dayAgo, cancellationToken);
            
        var pendingCount = await _context.Artists
            .CountAsync(a => a.VerificationStatus == VerificationStatus.Pending, cancellationToken);

        var summary = new DashboardSummaryDto(
            totalTracks,
            totalArtists,
            totalAlbums,
            totalUsers,
            newUsers24h,
            pendingCount
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

        return new AdminDashboardDto(summary, recentTracks, pendingArtists);
    }
}
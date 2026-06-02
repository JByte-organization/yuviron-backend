using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Dashboard.Queries.GetDashboardStats;

public sealed class GetStudioArtistDashboardHandler : IRequestHandler<GetStudioArtistDashboardQuery, StudioArtistDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public GetStudioArtistDashboardHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<StudioArtistDashboardDto> Handle(GetStudioArtistDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var membership = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.UserId == userId)
            .OrderBy(tm => tm.Role == ArtistTeamRole.Owner ? 0 :
                tm.Role == ArtistTeamRole.Manager ? 1 :
                tm.Role == ArtistTeamRole.Editor ? 2 :
                tm.Role == ArtistTeamRole.Viewer ? 3 : 4)
            .ThenByDescending(tm => tm.CreatedAt)
            .Select(tm => new CurrentArtistMembership(tm.ArtistId, tm.Role))
            .FirstOrDefaultAsync(cancellationToken);

        if (membership is null)
        {
            throw new NotFoundException(nameof(Artist), $"for user {userId}");
        }

        var artist = await _context.Artists
            .AsNoTracking()
            .Where(a => a.Id == membership.ArtistId)
            .Select(a => new ArtistDashboardSnapshot(
                a.Id,
                a.Name,
                a.AvatarUrl,
                a.BannerUrl,
                a.VerificationStatus,
                a.AlbumArtists
                    .Where(aa => !aa.Album.IsDeleted)
                    .Select(aa => aa.AlbumId)
                    .Distinct()
                    .Count(),
                a.TrackArtists
                    .Where(ta => !ta.Track.IsDeleted)
                    .Select(ta => ta.TrackId)
                    .Distinct()
                    .Count(),
                a.TeamMembers.Count(),
                a.TotalPlays,
                a.MonthlyListenersCount))
            .FirstOrDefaultAsync(cancellationToken);

        if (artist is null)
        {
            throw new NotFoundException(nameof(Artist), membership.ArtistId);
        }

        var totalFollowers = await _context.UserFollowArtists
            .AsNoTracking()
            .CountAsync(ufa => ufa.ArtistId == artist.Id, cancellationToken);

        var activeSubscription = await _context.ArtistSubscriptions
            .AsNoTracking()
            .Where(s => s.ArtistId == artist.Id &&
                        s.Status == SubscriptionStatus.Active &&
                        s.EndAt > utcNow)
            .OrderByDescending(s => s.EndAt)
            .Select(s => new StudioArtistSubscriptionDto(
                s.Id,
                s.PlanId,
                s.Plan.Name,
                s.StartAt,
                s.EndAt,
                s.IsAutoRenewing))
            .FirstOrDefaultAsync(cancellationToken);

        return new StudioArtistDashboardDto(
            new StudioArtistInfoDto(
                artist.Id,
                artist.Name,
                artist.AvatarUrl,
                artist.BannerUrl,
                artist.VerificationStatus,
                membership.Role),
            new StudioArtistSummaryDto(
                artist.TotalAlbums,
                artist.TotalTracks,
                totalFollowers,
                artist.TeamMembersCount,
                artist.TotalPlays,
                artist.MonthlyListenersCount),
            activeSubscription);
    }

    private sealed record CurrentArtistMembership(Guid ArtistId, ArtistTeamRole Role);

    private sealed record ArtistDashboardSnapshot(
        Guid Id,
        string Name,
        string? AvatarUrl,
        string? BannerUrl,
        VerificationStatus VerificationStatus,
        int TotalAlbums,
        int TotalTracks,
        int TeamMembersCount,
        long TotalPlays,
        int MonthlyListenersCount);
}

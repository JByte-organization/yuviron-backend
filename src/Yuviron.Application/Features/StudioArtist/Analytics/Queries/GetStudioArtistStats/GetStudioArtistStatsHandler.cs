using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetStudioArtistStats;

public sealed class GetStudioArtistStatsHandler : IRequestHandler<GetStudioArtistStatsQuery, StudioArtistStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cacheService; 

    public GetStudioArtistStatsHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser,
        ICacheService cacheService)
    {
        _context = context;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task<StudioArtistStatsDto> Handle(GetStudioArtistStatsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasAccess = await _context.ArtistTeamMembers
            .HasManagementAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to view this artist's statistics.");

        string cacheKey = $"studio:artist:{request.ArtistId}:stats";

        var cachedStats = await _cacheService.GetAsync<StudioArtistStatsDto>(cacheKey, cancellationToken);
        if (cachedStats != null) return cachedStats;

        var artist = await _context.Artists
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var totalAlbums = await _context.Albums
            .AsNoTracking()
            .CountAsync(a => a.AlbumArtists.Any(aa => aa.ArtistId == request.ArtistId), cancellationToken);

        var tracksQuery = _context.Tracks
            .AsNoTracking()
            .Where(t => t.TrackArtists.Any(ta => ta.ArtistId == request.ArtistId));

        var totalTracks = await tracksQuery.CountAsync(cancellationToken);

        var topTrack = await tracksQuery
            .OrderByDescending(t => t.PlayCount)
            .Select(t => new TopTrackStatDto(
                t.Id,
                t.Title,
                t.CoverUrl ?? t.Album!.CoverUrl, 
                t.PlayCount
            ))
            .FirstOrDefaultAsync(cancellationToken);

        var result = new StudioArtistStatsDto(
            artist.TotalPlays,
            artist.MonthlyListenersCount,
            totalAlbums,
            totalTracks,
            topTrack
        );

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(3), cancellationToken);

        return result;
    }
}
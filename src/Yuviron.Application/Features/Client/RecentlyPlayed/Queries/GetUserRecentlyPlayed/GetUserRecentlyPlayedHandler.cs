using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Client.RecentlyPlayed.Queries.GetUserRecentlyPlayed;

public sealed class GetUserRecentlyPlayedHandler : IRequestHandler<GetUserRecentlyPlayedQuery, List<RecentlyPlayedTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public GetUserRecentlyPlayedHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
    }

    public async Task<List<RecentlyPlayedTrackDto>> Handle(GetUserRecentlyPlayedQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId 
                     ?? throw new UnauthorizedAccessException();

        var recentTracksData = await _context.ListeningEvents
            .Where(le => le.UserId == userId)
            .GroupBy(le => le.TrackId)
            .Select(g => new
            {
                TrackId = g.Key,
                LastPlayed = g.Max(le => le.PlayedAt)
            })
            .OrderByDescending(x => x.LastPlayed)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        if (!recentTracksData.Any()) return new List<RecentlyPlayedTrackDto>();

        var trackIds = recentTracksData.Select(x => x.TrackId).ToList();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var dbTracks = await _context.Tracks
            .AvailableForPublic(utcNow) 
            .Where(t => trackIds.Contains(t.Id))
            .Select(t => new
            {
                t.Id,
                t.Title,
                Artists = t.TrackArtists.Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role)),
                CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null)
            })
            .ToListAsync(cancellationToken);

        return recentTracksData
            .Select(data => {
                var track = dbTracks.FirstOrDefault(t => t.Id == data.TrackId);
                if (track == null) return null;

                return new RecentlyPlayedTrackDto(
                    track.Id,
                    track.Title,
                    track.Artists, 
                    track.CoverUrl,
                    data.LastPlayed
                );
            })
            .Where(x => x != null)
            .ToList()!;
    }
}
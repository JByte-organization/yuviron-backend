using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.RecentlyPlayed.Queries.GetUserRecentlyPlayed;

public sealed class GetUserRecentlyPlayedHandler : IRequestHandler<GetUserRecentlyPlayedQuery, List<RecentlyPlayedTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserRecentlyPlayedHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
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

        var dbTracks = await _context.Tracks
            .AsNoTracking()
            .Where(t => trackIds.Contains(t.Id))
            .Select(t => new
            {
                t.Id,
                t.Title,
                ArtistNamesList = t.TrackArtists.Select(ta => ta.Artist.Name).ToList(),
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
                    string.Join(", ", track.ArtistNamesList),
                    track.CoverUrl,
                    data.LastPlayed
                );
            })
            .Where(x => x != null)
            .ToList()!;
    }
}
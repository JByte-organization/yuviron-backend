using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Client.RecentlyPlayed.Queries.GetUserRecentlyPlayed;

public sealed class GetUserRecentlyPlayedHandler : IRequestHandler<GetUserRecentlyPlayedQuery, List<RecentlyPlayedTrackDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public GetUserRecentlyPlayedHandler(
        ICatalogContext catalogContext, ISystemContext systemContext, 
        ICurrentUserService currentUserService,
        TimeProvider timeProvider)
    {
        _catalogContext = catalogContext;
        _systemContext = systemContext;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
    }

    public async Task<List<RecentlyPlayedTrackDto>> Handle(GetUserRecentlyPlayedQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId 
                     ?? throw new UnauthorizedAccessException();

        var recentTracksData = await _systemContext.ListeningEvents
            .AsNoTracking()
            .Where(le => le.UserId == userId && !le.IsPrivate)
            .GroupBy(le => le.TrackId)
            .Select(g => new
            {
                TrackId = g.Key,
                LastPlayed = g.Max(le => le.PlayedAt)
            })
            .OrderByDescending(x => x.LastPlayed)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        if (recentTracksData.Count == 0) 
            return new List<RecentlyPlayedTrackDto>();

        var trackIds = recentTracksData.Select(x => x.TrackId).ToList();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var dbTracksDict = await _catalogContext.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow) 
            .Where(t => trackIds.Contains(t.Id))
            .Select(t => new
            {
                t.Id,
                t.Title,
                Artists = t.TrackArtists
                    
                    .Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role)),
                CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null)
            })
            .ToDictionaryAsync(t => t.Id, cancellationToken); 

        return recentTracksData
            .Where(data => dbTracksDict.ContainsKey(data.TrackId)) 
            .Select(data => 
            {
                var track = dbTracksDict[data.TrackId];
                return new RecentlyPlayedTrackDto(
                    track.Id,
                    track.Title,
                    track.Artists, 
                    track.CoverUrl,
                    data.LastPlayed
                );
            })
            .ToList();
    }
}


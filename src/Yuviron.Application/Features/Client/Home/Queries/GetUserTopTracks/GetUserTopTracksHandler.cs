using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common.Models; 
using Yuviron.Application.Extensions; 

namespace Yuviron.Application.Features.Client.Home.Queries.GetUserTopTracks;

public sealed class GetUserTopTracksHandler : IRequestHandler<GetUserTopTracksQuery, List<TopTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public GetUserTopTracksHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
    }

    public async Task<List<TopTrackDto>> Handle(GetUserTopTracksQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId 
                     ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var minDate = utcNow.AddDays(-30); 
        
        var topTrackIds = await _context.ListeningEvents
            .Where(le => le.UserId == userId && 
                         le.PlayedAt >= minDate && 
                         le.MsPlayed >= 30000)
            .GroupBy(le => le.TrackId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        if (!topTrackIds.Any()) return new List<TopTrackDto>();

        var dbTracks = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(t => topTrackIds.Contains(t.Id))
            .Select(t => new
            {
                t.Id,
                t.Title,
                Artists = t.TrackArtists.Select(ta => new SimpleArtistDto(ta.Artist.Id, ta.Artist.Name)), 
                CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null)
            })
            .ToListAsync(cancellationToken);

        return topTrackIds
            .Select(id => dbTracks.FirstOrDefault(t => t.Id == id))
            .Where(t => t != null)
            .Select(t => new TopTrackDto(
                t!.Id,
                t.Title,
                t.Artists,
                t.CoverUrl
            ))
            .ToList();
    }
}
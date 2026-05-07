using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Extensions; 
using Yuviron.Application.Common.Models; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackRecommendations;

public sealed class GetTrackRecommendationsHandler : IRequestHandler<GetTrackRecommendationsQuery, List<RecommendedTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStreamTokenService _streamTokenService; 
    private readonly TimeProvider _timeProvider;            

    public GetTrackRecommendationsHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser,
        IStreamTokenService streamTokenService,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _streamTokenService = streamTokenService;
        _timeProvider = timeProvider;
    }

    public async Task<List<RecommendedTrackDto>> Handle(GetTrackRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var baseTrackTags = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id == request.TrackId)
            .Select(t => new {
                ArtistIds = t.TrackArtists.Select(a => a.ArtistId),
                GenreIds = t.TrackGenres.Select(g => g.GenreId),
                MoodIds = t.TrackMoods.Select(m => m.MoodId)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (baseTrackTags == null) throw new NotFoundException("Track", request.TrackId);

        var artistIds = baseTrackTags.ArtistIds.ToList();
        var genreIds = baseTrackTags.GenreIds.ToList();
        var moodIds = baseTrackTags.MoodIds.ToList();

        var rawRecommendations = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow) 
            .Where(t => t.Id != request.TrackId 
                     && (t.TrackArtists.Any(a => artistIds.Contains(a.ArtistId)) ||
                         t.TrackGenres.Any(g => genreIds.Contains(g.GenreId)) ||
                         t.TrackMoods.Any(m => moodIds.Contains(m.MoodId))))
            .Select(t => new {
                Track = new {
                    t.Id,
                    t.Title,
                    t.DurationMs,
                    t.Explicit,
                    CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                    FileKey = !string.IsNullOrWhiteSpace(t.HlsPlaylistUrl) ? t.HlsPlaylistUrl : t.AudioStorageKey,
                    Artists = t.TrackArtists.Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role)),
                    t.PlayCount
                },
                MatchScore = (t.TrackArtists.Any(a => artistIds.Contains(a.ArtistId)) ? 3 : 0) +
                             (t.TrackGenres.Any(g => genreIds.Contains(g.GenreId)) ? 2 : 0) +
                             (t.TrackMoods.Any(m => moodIds.Contains(m.MoodId)) ? 1 : 0)
            })
            .OrderByDescending(x => x.MatchScore)
            .ThenByDescending(x => x.Track.PlayCount)
            .Take(request.Limit)
            .Select(x => x.Track)
            .ToListAsync(cancellationToken);

        bool isAuthenticated = _currentUser.UserId.HasValue;
        var expiration = _timeProvider.GetUtcNow().AddHours(6);
        var expUnix = expiration.ToUnixTimeSeconds();

        return rawRecommendations.Select(t => 
        {
            string? audioUrl = null;
            if (isAuthenticated && !string.IsNullOrWhiteSpace(t.FileKey))
            {
                var signature = _streamTokenService.GenerateToken(t.Id, expiration);
                var fileName = Path.GetFileName(t.FileKey);
                audioUrl = $"/api/stream/tracks/{t.Id}/{fileName}?exp={expUnix}&sig={signature}";
            }

            return new RecommendedTrackDto(
                t.Id, t.Title, t.DurationMs, t.Explicit,
                t.CoverUrl, audioUrl, t.Artists
            );
        }).ToList();
    }
}
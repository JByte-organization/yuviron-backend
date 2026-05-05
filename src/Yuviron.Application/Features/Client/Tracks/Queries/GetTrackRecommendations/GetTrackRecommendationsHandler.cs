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

        var rawRecommendations = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow) 
            .Where(t => t.Id != request.TrackId 
                     && (t.TrackArtists.Any(a => baseTrackTags.ArtistIds.Contains(a.ArtistId)) ||
                         t.TrackGenres.Any(g => baseTrackTags.GenreIds.Contains(g.GenreId)) ||
                         t.TrackMoods.Any(m => baseTrackTags.MoodIds.Contains(m.MoodId))))
            .OrderByDescending(t => t.PlayCount) 
            .Take(request.Limit)
            .Select(t => new {
                t.Id,
                t.Title,
                t.DurationMs,
                t.Explicit,
                CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                FileKey = !string.IsNullOrWhiteSpace(t.HlsPlaylistUrl) ? t.HlsPlaylistUrl : t.AudioStorageKey,
                Artists = t.TrackArtists.Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role))
            })
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
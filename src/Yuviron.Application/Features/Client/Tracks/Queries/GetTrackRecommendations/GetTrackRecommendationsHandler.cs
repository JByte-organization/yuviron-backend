using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Entities;

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
        var baseTrack = await _context.Tracks
            .Include(t => t.TrackArtists)
            .Include(t => t.TrackGenres)
            .Include(t => t.TrackMoods)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken);

        if (baseTrack == null)
        {
            throw new NotFoundException(nameof(Track), request.TrackId);
        }

        var artistIds = baseTrack.TrackArtists.Select(a => a.ArtistId).ToList();
        var genreIds = baseTrack.TrackGenres.Select(g => g.GenreId).ToList();
        var moodIds = baseTrack.TrackMoods.Select(m => m.MoodId).ToList();

        var recommendedTrackIds = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id != request.TrackId 
                     && !t.IsDeleted 
                     && t.VisibilityStatus == VisibilityStatus.Published
                     && t.ProcessingStatus == TrackProcessingStatus.Ready
                     && (
                         t.TrackArtists.Any(a => artistIds.Contains(a.ArtistId)) ||
                         t.TrackGenres.Any(g => genreIds.Contains(g.GenreId)) ||
                         t.TrackMoods.Any(m => moodIds.Contains(m.MoodId))
                     ))
            .OrderByDescending(t => t.PlayCount) 
            .Select(t => t.Id) 
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        if (!recommendedTrackIds.Any())
        {
            return new List<RecommendedTrackDto>();
        }

        var tracks = await _context.Tracks
            .Include(t => t.Album)
            .Include(t => t.TrackArtists)
                .ThenInclude(ta => ta.Artist)
            .AsNoTracking()
            .Where(t => recommendedTrackIds.Contains(t.Id)) 
            .ToListAsync(cancellationToken);

        tracks = tracks.OrderByDescending(t => t.PlayCount).ToList();

        bool isAuthenticated = _currentUser.UserId.HasValue;

        var expiration = _timeProvider.GetUtcNow().AddHours(6);
        var expUnix = expiration.ToUnixTimeSeconds();

        var recommendations = tracks.Select(t => 
        {
            string? audioUrl = null;
            var fileKey = !string.IsNullOrWhiteSpace(t.HlsPlaylistUrl) ? t.HlsPlaylistUrl : t.AudioStorageKey;

            if (isAuthenticated && !string.IsNullOrWhiteSpace(fileKey))
            {
                var signature = _streamTokenService.GenerateToken(t.Id, expiration);
                var fileName = Path.GetFileName(fileKey);
                audioUrl = $"/api/stream/tracks/{t.Id}/{fileName}?exp={expUnix}&sig={signature}";
            }

            return new RecommendedTrackDto(
                t.Id,
                t.Title,
                t.DurationMs,
                t.Explicit,
                t.CoverUrl ?? t.Album?.CoverUrl,
                audioUrl,
                t.TrackArtists.Select(ta => new RecommendedTrackArtistDto(
                    ta.Artist.Id,
                    ta.Artist.Name
                )).ToList() 
            );
        }).ToList();

        return recommendations;
    }
}
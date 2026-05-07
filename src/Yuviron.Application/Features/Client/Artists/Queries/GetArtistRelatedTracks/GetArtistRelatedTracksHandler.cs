using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistRelatedTracks;

public sealed class GetArtistRelatedTracksHandler : IRequestHandler<GetArtistRelatedTracksQuery, List<RelatedTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStreamTokenService _streamTokenService;
    private readonly TimeProvider _timeProvider;

    public GetArtistRelatedTracksHandler(
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

    public async Task<List<RelatedTrackDto>> Handle(GetArtistRelatedTracksQuery request, CancellationToken cancellationToken)
    {
        var artistExists = await _context.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArtistId && !a.IsDeleted, cancellationToken);

        if (!artistExists)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var artistGenreIds = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .ForArtist(request.ArtistId)
            .SelectMany(t => t.TrackGenres.Select(tg => tg.GenreId))
            .Distinct()
            .ToListAsync(cancellationToken);

        if (artistGenreIds.Count == 0)
        {
            return new List<RelatedTrackDto>();
        }

        var rawTracks = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(t => !t.TrackArtists.Any(ta => ta.ArtistId == request.ArtistId) &&
                        t.TrackGenres.Any(tg => artistGenreIds.Contains(tg.GenreId)))
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.DurationMs,
                t.Explicit,
                CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                FileKey = !string.IsNullOrWhiteSpace(t.HlsPlaylistUrl) ? t.HlsPlaylistUrl : t.AudioStorageKey,
                SharedGenresCount = t.TrackGenres.Count(tg => artistGenreIds.Contains(tg.GenreId)),
                t.PlayCount,
                Artists = t.TrackArtists.Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role))
            })
            .OrderByDescending(t => t.SharedGenresCount)
            .ThenByDescending(t => t.PlayCount)
            .ThenBy(t => t.Title)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        var isAuthenticated = _currentUser.UserId.HasValue;
        var expiration = _timeProvider.GetUtcNow().AddHours(6);
        var expUnix = expiration.ToUnixTimeSeconds();

        return rawTracks
            .Select(t =>
            {
                string? audioUrl = null;

                if (isAuthenticated && !string.IsNullOrWhiteSpace(t.FileKey))
                {
                    var signature = _streamTokenService.GenerateToken(t.Id, expiration);
                    var fileName = Path.GetFileName(t.FileKey);
                    audioUrl = $"/api/stream/tracks/{t.Id}/{fileName}?exp={expUnix}&sig={signature}";
                }

                return new RelatedTrackDto(
                    t.Id,
                    t.Title,
                    t.DurationMs,
                    t.Explicit,
                    t.CoverUrl,
                    audioUrl,
                    t.Artists
                );
            })
            .ToList();
    }
}

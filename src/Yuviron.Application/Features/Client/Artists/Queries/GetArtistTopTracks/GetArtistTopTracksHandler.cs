using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Security; 
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistTopTracks;

public sealed class GetArtistTopTracksHandler : IRequestHandler<GetArtistTopTracksQuery, List<ArtistTopTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStreamTokenService _streamTokenService; 
    private readonly TimeProvider _timeProvider;            

    public GetArtistTopTracksHandler(
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

    public async Task<List<ArtistTopTrackDto>> Handle(GetArtistTopTracksQuery request, CancellationToken cancellationToken)
    {
        bool artistExists = await _context.Artists
            .AnyAsync(a => a.Id == request.ArtistId && !a.IsDeleted, cancellationToken);

        if (!artistExists)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        bool isAuthenticated = _currentUser.UserId.HasValue;

        var rawTracks = await _context.Tracks
            .AsNoTracking()
            .Where(t => !t.IsDeleted 
                     && t.VisibilityStatus == VisibilityStatus.Published
                     && t.ProcessingStatus == TrackProcessingStatus.Ready
                     && t.TrackArtists.Any(ta => ta.ArtistId == request.ArtistId))
            .OrderByDescending(t => t.PlayCount)
            .Take(request.Limit)
            .Select(t => new 
            {
                t.Id,
                t.Title,
                t.DurationMs,
                t.Explicit,
                CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                FileKey = !string.IsNullOrWhiteSpace(t.HlsPlaylistUrl) ? t.HlsPlaylistUrl : t.AudioStorageKey,
                t.PlayCount,
                t.AlbumId,
                AlbumTitle = t.Album != null ? t.Album.Title : "Unknown Album",
                Artists = t.TrackArtists.Select(ta => new ArtistTopTrackArtistDto(
                    ta.Artist.Id,
                    ta.Artist.Name
                )).ToList()
            })
            .ToListAsync(cancellationToken);

        var expiration = _timeProvider.GetUtcNow().AddHours(6);
        var expUnix = expiration.ToUnixTimeSeconds();

        return rawTracks.Select(t => 
        {
            string? audioUrl = null;
            if (isAuthenticated && !string.IsNullOrWhiteSpace(t.FileKey))
            {
                var signature = _streamTokenService.GenerateToken(t.Id, expiration);
                var fileName = Path.GetFileName(t.FileKey);
                audioUrl = $"/api/stream/tracks/{t.Id}/{fileName}?exp={expUnix}&sig={signature}";
            }

            return new ArtistTopTrackDto(
                t.Id,
                t.Title,
                t.DurationMs,
                t.Explicit,
                t.CoverUrl,
                audioUrl,
                t.PlayCount,
                t.AlbumId,
                t.AlbumTitle,
                t.Artists
            );
        }).ToList();
    }
}
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistTopTracks;

public sealed class GetArtistTopTracksHandler : IRequestHandler<GetArtistTopTracksQuery, List<ArtistTopTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetArtistTopTracksHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
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

        var topTracks = await _context.Tracks
            .AsNoTracking()
            .Where(t => !t.IsDeleted 
                     && t.VisibilityStatus == VisibilityStatus.Published
                     && t.ProcessingStatus == TrackProcessingStatus.Ready
                     && t.TrackArtists.Any(ta => ta.ArtistId == request.ArtistId))
            .OrderByDescending(t => t.PlayCount)
            .Take(request.Limit)
            .Select(t => new ArtistTopTrackDto(
                t.Id,
                t.Title,
                t.DurationMs,
                t.Explicit,
                t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                isAuthenticated 
                    ? (!string.IsNullOrWhiteSpace(t.HlsPlaylistUrl) ? t.HlsPlaylistUrl : t.AudioStorageKey) 
                    : null,
                t.PlayCount,
                t.AlbumId,
                t.Album != null ? t.Album.Title : "Unknown Album",
                t.TrackArtists.Select(ta => new ArtistTopTrackArtistDto(
                    ta.Artist.Id,
                    ta.Artist.Name
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return topTracks;
    }
}
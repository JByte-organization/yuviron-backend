using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistTopTracks;

public sealed class GetArtistTopTracksHandler : IRequestHandler<GetArtistTopTracksQuery, List<ArtistTopTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetArtistTopTracksHandler(
        IApplicationDbContext context,
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<List<ArtistTopTrackDto>> Handle(GetArtistTopTracksQuery request, CancellationToken cancellationToken)
    {
        var artistExists = await _context.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArtistId && !a.IsDeleted, cancellationToken);

        if (!artistExists)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var rawTracks = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .ForArtist(request.ArtistId)
            .OrderByDescending(t => t.PlayCount)
            .ThenBy(t => t.Title)
            .Take(request.Limit)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.DurationMs,
                t.Explicit,
                CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                t.PlayCount,
                t.AlbumId,
                AlbumTitle = t.Album != null ? t.Album.Title : "Unknown Album",
                Artists = t.TrackArtists
                    .Where(ta => !ta.Artist.IsDeleted)
                    .Select(ta => new SimpleArtistDto(ta.Artist.Id, ta.Artist.Name))
            })
            .ToListAsync(cancellationToken);
        
        return rawTracks.Select(t => new ArtistTopTrackDto(
            t.Id,
            t.Title,
            t.DurationMs,
            t.Explicit,
            t.CoverUrl,
            t.PlayCount,
            t.AlbumId,
            t.AlbumTitle,
            t.Artists
        )).ToList();
    }
}

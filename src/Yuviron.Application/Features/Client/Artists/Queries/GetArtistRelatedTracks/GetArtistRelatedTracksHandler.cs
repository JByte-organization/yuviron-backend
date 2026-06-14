using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;

    public GetArtistRelatedTracksHandler(
        ICatalogContext catalogContext,
        TimeProvider timeProvider)
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
    }

    public async Task<List<RelatedTrackDto>> Handle(GetArtistRelatedTracksQuery request, CancellationToken cancellationToken)
    {
        var artistExists = await _catalogContext.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArtistId , cancellationToken);

        if (!artistExists)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var artistGenreIds = await _catalogContext.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .ForArtistMain(request.ArtistId)
            .SelectMany(t => t.TrackGenres
                
                .Select(tg => tg.GenreId))
            .Distinct()
            .ToListAsync(cancellationToken);

        if (artistGenreIds.Count == 0)
        {
            return new List<RelatedTrackDto>();
        }

        var rawTracks = await _catalogContext.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(t => !t.TrackArtists.Any(ta => ta.ArtistId == request.ArtistId) &&
                        t.TrackGenres.Any(tg => !tg.Genre.IsDeleted && artistGenreIds.Contains(tg.GenreId)))
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.DurationMs,
                t.Explicit,
                CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                FileKey = !string.IsNullOrWhiteSpace(t.HlsPlaylistUrl) ? t.HlsPlaylistUrl : t.AudioStorageKey,
                SharedGenresCount = t.TrackGenres.Count(tg => !tg.Genre.IsDeleted && artistGenreIds.Contains(tg.GenreId)),
                t.PlayCount,
                Artists = t.TrackArtists
                    
                    .Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role))
            })
            .OrderByDescending(t => t.SharedGenresCount)
            .ThenByDescending(t => t.PlayCount)
            .ThenBy(t => t.Title)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        
        return rawTracks
            .Select(t =>
            {
                return new RelatedTrackDto(
                    t.Id,
                    t.Title,
                    t.DurationMs,
                    t.Explicit,
                    t.CoverUrl,
                    t.Artists
                );
            })
            .ToList();
    }
}

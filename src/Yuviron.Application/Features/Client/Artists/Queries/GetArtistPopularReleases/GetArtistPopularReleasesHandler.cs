using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistPopularReleases;

public sealed class GetArtistPopularReleasesHandler : IRequestHandler<GetArtistPopularReleasesQuery, List<ArtistAlbumDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetArtistPopularReleasesHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<List<ArtistAlbumDto>> Handle(GetArtistPopularReleasesQuery request, CancellationToken cancellationToken)
    {
        var artistExists = await _context.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArtistId && !a.IsDeleted, cancellationToken);

        if (!artistExists)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var publicTracks = _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        return await _context.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .ForArtist(request.ArtistId)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.CoverUrl,
                a.ReleaseDate,
                PublicTracksCount = publicTracks.Count(t => t.AlbumId == a.Id),
                TotalPlays = publicTracks
                    .Where(t => t.AlbumId == a.Id)
                    .Sum(t => (long?)t.PlayCount) ?? 0
            })
            .Where(a => a.PublicTracksCount > 0)
            .OrderByDescending(a => a.TotalPlays)
            .ThenByDescending(a => a.ReleaseDate)
            .ThenBy(a => a.Title)
            .Take(request.Limit)
            .Select(a => new ArtistAlbumDto(
                a.Id,
                a.Title,
                a.CoverUrl,
                a.ReleaseDate.Year
            ))
            .ToListAsync(cancellationToken);
    }
}

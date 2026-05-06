using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;

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
        await ArtistQueries.EnsureArtistExistsAsync(_context, request.ArtistId, cancellationToken);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var publicTracks = _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        return await ArtistQueries.BuildPublicArtistReleasesQuery(_context, request.ArtistId, utcNow)
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

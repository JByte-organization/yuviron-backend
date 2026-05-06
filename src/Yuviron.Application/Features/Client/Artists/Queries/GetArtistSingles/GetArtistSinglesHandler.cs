using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistSingles;

public sealed class GetArtistSinglesHandler : IRequestHandler<GetArtistSinglesQuery, PaginatedList<ArtistAlbumDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetArtistSinglesHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<ArtistAlbumDto>> Handle(GetArtistSinglesQuery request, CancellationToken cancellationToken)
    {
        await ArtistQueries.EnsureArtistExistsAsync(_context, request.ArtistId, cancellationToken);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var publicTracks = _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        var projectedQuery = ArtistQueries.BuildPublicArtistReleasesQuery(_context, request.ArtistId, utcNow)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.CoverUrl,
                a.ReleaseDate,
                a.CreatedAt,
                PublicTracksCount = publicTracks.Count(t => t.AlbumId == a.Id)
            })
            .Where(a => a.PublicTracksCount == 1)
            .OrderByDescending(a => a.ReleaseDate)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new ArtistAlbumDto(
                a.Id,
                a.Title,
                a.CoverUrl,
                a.ReleaseDate.Year
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}

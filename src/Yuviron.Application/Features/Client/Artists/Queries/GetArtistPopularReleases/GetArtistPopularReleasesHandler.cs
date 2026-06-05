using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistPopularReleases;

public sealed class GetArtistPopularReleasesHandler : IRequestHandler<GetArtistPopularReleasesQuery, List<ArtistAlbumDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public GetArtistPopularReleasesHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICacheService cache,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<List<ArtistAlbumDto>> Handle(GetArtistPopularReleasesQuery request, CancellationToken cancellationToken)
    {
        var artistExists = await _context.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArtistId , cancellationToken);

        if (!artistExists)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var publicTracks = _context.Tracks.AsNoTracking().AvailableForPublic(utcNow);

        var albums = await _context.Albums
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
                a.ReleaseDate.Year,
                false 
            ))
            .ToListAsync(cancellationToken);

        return await albums.EnrichWithCacheAsync(
            _cache, 
            _currentUser.UserId, 
            "saved_albums", 
            x => x.Id, 
            (x, saved) => x with { IsSaved = saved }, 
            cancellationToken);
    }
}
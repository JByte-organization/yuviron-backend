using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistSimilarArtists;

public sealed class GetArtistSimilarArtistsHandler : IRequestHandler<GetArtistSimilarArtistsQuery, List<SimilarArtistDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public GetArtistSimilarArtistsHandler(
        ICatalogContext catalogContext, ILibraryContext libraryContext, 
        TimeProvider timeProvider,
        ICacheService cache,
        ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _timeProvider = timeProvider;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<List<SimilarArtistDto>> Handle(GetArtistSimilarArtistsQuery request, CancellationToken cancellationToken)
    {
        var artistExists = await _catalogContext.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArtistId , cancellationToken);

        if (!artistExists) throw new NotFoundException(nameof(Artist), request.ArtistId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var artistGenreIds = await _catalogContext.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .ForArtistMain(request.ArtistId)
            .SelectMany(t => t.TrackGenres.Select(tg => tg.GenreId))
            .Distinct()
            .ToListAsync(cancellationToken);

        if (artistGenreIds.Count == 0) return new List<SimilarArtistDto>();

        var artists = await _catalogContext.Artists
            .AsNoTracking()
            .Where(a => a.Id != request.ArtistId )
            .Where(a => a.TrackArtists.Any(ta =>
                !ta.Track.IsDeleted &&
                ta.Track.VisibilityStatus == VisibilityStatus.Published &&
                ta.Track.ProcessingStatus == TrackProcessingStatus.Ready &&
                ta.Track.Album != null &&
                !ta.Track.Album.IsDeleted &&
                ta.Track.Album.VisibilityStatus == VisibilityStatus.Published &&
                ta.Track.Album.ReleaseDate <= utcNow &&
                ta.Track.TrackGenres.Any(tg => !tg.Genre.IsDeleted && artistGenreIds.Contains(tg.GenreId))))
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.AvatarUrl,
                a.VerificationStatus,
                a.TotalPlays,
                SharedGenresCount = a.TrackArtists
                    .Where(ta =>
                        !ta.Track.IsDeleted &&
                        ta.Track.VisibilityStatus == VisibilityStatus.Published &&
                        ta.Track.ProcessingStatus == TrackProcessingStatus.Ready &&
                        ta.Track.Album != null &&
                        !ta.Track.Album.IsDeleted &&
                        ta.Track.Album.VisibilityStatus == VisibilityStatus.Published &&
                        ta.Track.Album.ReleaseDate <= utcNow)
                    .SelectMany(ta => ta.Track.TrackGenres
                        .Where(tg => !tg.Genre.IsDeleted && artistGenreIds.Contains(tg.GenreId))
                        .Select(tg => tg.GenreId))
                    .Distinct()
                    .Count(),
                FollowersCount = _libraryContext.UserFollowArtists.Count(ufa => ufa.ArtistId == a.Id)
            })
            .OrderByDescending(x => x.SharedGenresCount)
            .ThenByDescending(x => x.TotalPlays)
            .ThenBy(x => x.Name)
            .Take(request.Limit)
            .Select(x => new SimilarArtistDto(
                x.Id,
                x.Name,
                x.AvatarUrl,
                x.VerificationStatus,
                x.FollowersCount,
                false 
            ))
            .ToListAsync(cancellationToken);

        return await artists.EnrichWithCacheAsync(
            _cache, 
            _currentUser.UserId, 
            "followed_artists", 
            x => x.Id, 
            (x, followed) => x with { IsFollowed = followed }, 
            cancellationToken);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistSimilarArtists;

public sealed class GetArtistSimilarArtistsHandler : IRequestHandler<GetArtistSimilarArtistsQuery, List<SimilarArtistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetArtistSimilarArtistsHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<List<SimilarArtistDto>> Handle(GetArtistSimilarArtistsQuery request, CancellationToken cancellationToken)
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
            .SelectMany(t => t.TrackGenres
                .Where(tg => !tg.Genre.IsDeleted)
                .Select(tg => tg.GenreId))
            .Distinct()
            .ToListAsync(cancellationToken);

        if (artistGenreIds.Count == 0)
        {
            return new List<SimilarArtistDto>();
        }

        return await _context.Artists
            .AsNoTracking()
            .Where(a => a.Id != request.ArtistId && !a.IsDeleted)
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
                FollowersCount = _context.UserFollowArtists.Count(ufa => ufa.ArtistId == a.Id)
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
                x.FollowersCount
            ))
            .ToListAsync(cancellationToken);
    }
}

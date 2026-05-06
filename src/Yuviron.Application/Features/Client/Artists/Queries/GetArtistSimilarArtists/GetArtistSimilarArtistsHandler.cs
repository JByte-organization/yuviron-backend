using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;

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
        await ArtistQueries.EnsureArtistExistsAsync(_context, request.ArtistId, cancellationToken);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var artistGenreIds = await ArtistQueries.BuildPublicArtistTracksQuery(_context, request.ArtistId, utcNow)
            .SelectMany(t => t.TrackGenres.Select(tg => tg.GenreId))
            .Distinct()
            .ToListAsync(cancellationToken);

        if (artistGenreIds.Count == 0)
        {
            return new List<SimilarArtistDto>();
        }

        return await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(t => t.TrackGenres.Any(tg => artistGenreIds.Contains(tg.GenreId)))
            .SelectMany(t => t.TrackArtists
                .Where(ta => ta.ArtistId != request.ArtistId && !ta.Artist.IsDeleted)
                .SelectMany(ta => t.TrackGenres
                    .Where(tg => artistGenreIds.Contains(tg.GenreId))
                    .Select(tg => new
                    {
                        ta.ArtistId,
                        ta.Artist.Name,
                        ta.Artist.AvatarUrl,
                        ta.Artist.VerificationStatus,
                        ta.Artist.TotalPlays,
                        GenreId = tg.GenreId
                    })))
            .GroupBy(x => new
            {
                x.ArtistId,
                x.Name,
                x.AvatarUrl,
                x.VerificationStatus,
                x.TotalPlays
            })
            .Select(g => new
            {
                g.Key.ArtistId,
                g.Key.Name,
                g.Key.AvatarUrl,
                g.Key.VerificationStatus,
                g.Key.TotalPlays,
                SharedGenresCount = g.Select(x => x.GenreId).Distinct().Count(),
                FollowersCount = _context.UserFollowArtists.Count(ufa => ufa.ArtistId == g.Key.ArtistId)
            })
            .OrderByDescending(x => x.SharedGenresCount)
            .ThenByDescending(x => x.TotalPlays)
            .ThenBy(x => x.Name)
            .Take(request.Limit)
            .Select(x => new SimilarArtistDto(
                x.ArtistId,
                x.Name,
                x.AvatarUrl,
                x.VerificationStatus,
                x.FollowersCount
            ))
            .ToListAsync(cancellationToken);
    }
}

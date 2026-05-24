using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; 
using Yuviron.Application.Common.Models; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackRecommendations;

public sealed class GetTrackRecommendationsHandler : IRequestHandler<GetTrackRecommendationsQuery, List<RecommendedTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;            

    public GetTrackRecommendationsHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<List<RecommendedTrackDto>> Handle(GetTrackRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var baseTrackTags = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id == request.TrackId)
            .Select(t => new {
                ArtistIds = t.TrackArtists.Select(a => a.ArtistId),
                GenreIds = t.TrackGenres.Select(g => g.GenreId),
                MoodIds = t.TrackMoods.Select(m => m.MoodId)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (baseTrackTags == null) throw new NotFoundException("Track", request.TrackId);

        var artistIds = baseTrackTags.ArtistIds.ToList();
        var genreIds = baseTrackTags.GenreIds.ToList();
        var moodIds = baseTrackTags.MoodIds.ToList();

        var rawRecommendations = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow) 
            .Where(t => t.Id != request.TrackId 
                     && (t.TrackArtists.Any(a => !a.Artist.IsDeleted && artistIds.Contains(a.ArtistId)) ||
                         t.TrackGenres.Any(g => !g.Genre.IsDeleted && genreIds.Contains(g.GenreId)) ||
                         t.TrackMoods.Any(m => !m.Mood.IsDeleted && moodIds.Contains(m.MoodId))))
            .Select(t => new {
                Track = new {
                    t.Id,
                    t.Title,
                    t.DurationMs,
                    t.Explicit,
                    CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                    Artists = t.TrackArtists
                        
                        .Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role)),
                    t.PlayCount
                },
                MatchScore = (t.TrackArtists.Any(a => !a.Artist.IsDeleted && artistIds.Contains(a.ArtistId)) ? 3 : 0) +
                             (t.TrackGenres.Any(g => !g.Genre.IsDeleted && genreIds.Contains(g.GenreId)) ? 2 : 0) +
                             (t.TrackMoods.Any(m => !m.Mood.IsDeleted && moodIds.Contains(m.MoodId)) ? 1 : 0)
            })
            .OrderByDescending(x => x.MatchScore)
            .ThenByDescending(x => x.Track.PlayCount)
            .Take(request.Limit)
            .Select(x => x.Track)
            .ToListAsync(cancellationToken);
        
        return rawRecommendations.Select(t => new RecommendedTrackDto(
            t.Id, 
            t.Title, 
            t.DurationMs, 
            t.Explicit,
            t.CoverUrl, 
            t.Artists
        )).ToList();
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Home.Queries.GetPersonalizedRecommendations;

public class GetPersonalizedRecommendationsQueryHandler : IRequestHandler<GetPersonalizedRecommendationsQuery, List<RecommendationTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cacheService;
    private readonly TimeProvider _timeProvider;

    public GetPersonalizedRecommendationsQueryHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser, 
        ICacheService cacheService,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _cacheService = cacheService;
        _timeProvider = timeProvider;
    }

    public async Task<List<RecommendationTrackDto>> Handle(GetPersonalizedRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return new List<RecommendationTrackDto>();

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        string cacheKey = $"Recommendations:{userId.Value}";
        var cachedRecs = await _cacheService.GetAsync<List<RecommendationTrackDto>>(cacheKey, cancellationToken);
        if (cachedRecs != null)
        {
            return cachedRecs.Take(request.Limit).ToList();
        }

        var recentTrackIds = await _context.ListeningEvents
            .Where(le => le.UserId == userId.Value && !le.IsPrivate)
            .OrderByDescending(le => le.PlayedAt)
            .Take(50)
            .Select(le => le.TrackId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var likedTrackIds = await _context.UserSavedTracks
            .Where(ut => ut.UserId == userId.Value)
            .OrderByDescending(ut => ut.SavedAt)
            .Take(20)
            .Select(ut => ut.TrackId)
            .ToListAsync(cancellationToken);

        var seedTrackIds = recentTrackIds.Union(likedTrackIds).ToList();

        if (!seedTrackIds.Any())
        {
            return new List<RecommendationTrackDto>(); 
        }

        var seedArtists = await _context.TrackArtists
            .Where(ta => seedTrackIds.Contains(ta.TrackId))
            .Select(ta => ta.ArtistId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var seedGenres = await _context.TrackGenres
            .Where(tg => seedTrackIds.Contains(tg.TrackId))
            .Select(tg => tg.GenreId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var query = _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Include(t => t.TrackArtists).ThenInclude(ta => ta.Artist)
            .Include(t => t.TrackGenres)
            .Where(t => !seedTrackIds.Contains(t.Id));

        if (seedArtists.Any() && seedGenres.Any())
        {
            query = query.Where(t => 
                t.TrackArtists.Any(ta => seedArtists.Contains(ta.ArtistId)) ||
                t.TrackGenres.Any(tg => seedGenres.Contains(tg.GenreId)));
        }

        var recommendedTracks = await query
            .OrderByDescending(t => t.PlayCount)
            .Take(request.Limit * 2) 
            .ToListAsync(cancellationToken);

        var rng = new Random();
        var finalTracks = recommendedTracks.OrderBy(x => rng.Next()).Take(request.Limit).Select(t => new RecommendationTrackDto(
            t.Id,
            t.Title,
            t.DurationMs,
            t.Explicit,
            t.CoverUrl,
            t.TrackArtists.Select(ta => new TrackArtistDto(ta.ArtistId, ta.Artist.Name, ta.Role))
        )).ToList();

        await _cacheService.SetAsync(cacheKey, finalTracks, TimeSpan.FromHours(1), cancellationToken);

        return finalTracks;
    }
}


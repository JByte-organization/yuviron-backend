using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackRecommendations;

public sealed class GetTrackRecommendationsHandler : IRequestHandler<GetTrackRecommendationsQuery, List<RecommendedTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTrackRecommendationsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<RecommendedTrackDto>> Handle(GetTrackRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var baseTrack = await _context.Tracks
            .Include(t => t.TrackArtists)
            .Include(t => t.TrackGenres)
            .Include(t => t.TrackMoods)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken);

        if (baseTrack == null)
        {
            throw new NotFoundException(nameof(Track), request.TrackId);
        }

        var artistIds = baseTrack.TrackArtists.Select(a => a.ArtistId).ToList();
        var genreIds = baseTrack.TrackGenres.Select(g => g.GenreId).ToList();
        var moodIds = baseTrack.TrackMoods.Select(m => m.MoodId).ToList();

        // КРОК 1: Знаходимо ТІЛЬКИ ID рекомендованих треків (швидко і без помилок генерації SQL)
        var recommendedTrackIds = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id != request.TrackId 
                     && !t.IsDeleted 
                     && t.VisibilityStatus == VisibilityStatus.Published
                     && t.ProcessingStatus == TrackProcessingStatus.Ready
                     && (
                         t.TrackArtists.Any(a => artistIds.Contains(a.ArtistId)) ||
                         t.TrackGenres.Any(g => genreIds.Contains(g.GenreId)) ||
                         t.TrackMoods.Any(m => moodIds.Contains(m.MoodId))
                     ))
            .OrderByDescending(t => t.PlayCount) 
            .Select(t => t.Id) // <-- Беремо ТІЛЬКИ ідентифікатори
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        if (!recommendedTrackIds.Any())
        {
            return new List<RecommendedTrackDto>();
        }

        // КРОК 2: Витягуємо повні треки з усіма Include, використовуючи знайдені ID
        var tracks = await _context.Tracks
            .Include(t => t.Album)
            .Include(t => t.TrackArtists)
                .ThenInclude(ta => ta.Artist)
            .AsNoTracking()
            .Where(t => recommendedTrackIds.Contains(t.Id)) // <-- Проста і надійна умова
            .ToListAsync(cancellationToken);

        // Відновлюємо правильне сортування за популярністю, бо IN(...) може збити порядок
        tracks = tracks.OrderByDescending(t => t.PlayCount).ToList();

        bool isAuthenticated = _currentUser.UserId.HasValue;

        // Мапинг у DTO
        var recommendations = tracks.Select(t => new RecommendedTrackDto(
            t.Id,
            t.Title,
            t.DurationMs,
            t.Explicit,
            t.CoverUrl ?? t.Album?.CoverUrl,
            isAuthenticated 
                ? (!string.IsNullOrWhiteSpace(t.HlsPlaylistUrl) ? t.HlsPlaylistUrl : t.AudioStorageKey) 
                : null,
            t.TrackArtists.Select(ta => new RecommendedTrackArtistDto(
                ta.Artist.Id,
                ta.Artist.Name
            )).ToList() 
        )).ToList();

        return recommendations;
    }
}
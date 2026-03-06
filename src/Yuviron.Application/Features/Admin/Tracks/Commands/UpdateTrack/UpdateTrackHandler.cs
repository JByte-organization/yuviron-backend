using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrack;

public sealed class UpdateTrackCommandHandler : IRequestHandler<UpdateTrackCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateTrackCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateTrackCommand request, CancellationToken cancellationToken)
    {
        // 1. Ищем трек вместе со ВСЕМИ связями (Artists, Genres + Moods)
        // Если не сделать Include для Moods, SyncMoods не сможет удалить старые связи
        var track = await _context.Tracks
                        .Include(t => t.TrackArtists)
                        .Include(t => t.TrackGenres)
                        .Include(t => t.TrackMoods) 
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        // 2. Валидация альбома
        if (request.AlbumId.HasValue)
        {
            var albumExists = await _context.Albums.AsNoTracking()
                .AnyAsync(a => a.Id == request.AlbumId.Value, cancellationToken);
            if (!albumExists) throw new NotFoundException(nameof(Album), request.AlbumId.Value);
        }

        // 3. Валидация артистов
        var uniqueArtistIds = request.ArtistIds.Distinct().ToList();
        var existingArtists = await _context.Artists.CountAsync(a => uniqueArtistIds.Contains(a.Id), cancellationToken);
        if (existingArtists != uniqueArtistIds.Count) throw new ArgumentException("Invalid artists provided.");

        // 4. Валидация жанров
        var uniqueGenreIds = request.GenreIds.Distinct().ToList();
        var existingGenres = await _context.Genres.CountAsync(g => uniqueGenreIds.Contains(g.Id), cancellationToken);
        if (existingGenres != uniqueGenreIds.Count) throw new ArgumentException("Invalid genres provided.");

        // 5. Валидация настроений (Moods) — ДОБАВЛЕНО
        var uniqueMoodIds = request.MoodIds.Distinct().ToList();
        var existingMoods = await _context.Moods.CountAsync(m => uniqueMoodIds.Contains(m.Id), cancellationToken);
        if (existingMoods != uniqueMoodIds.Count) throw new ArgumentException("Invalid moods provided.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // 6. Обновляем доменную сущность (теперь со всеми параметрами)
        track.UpdateDetails(
            request.AlbumId,
            request.Title,
            request.DurationMs,
            request.Explicit,
            request.CoverUrl,
            request.AudioStorageKey,
            request.PreviewStorageKey,
            request.VisibilityStatus,
            uniqueArtistIds,
            uniqueGenreIds,
            uniqueMoodIds, // Передаем настроения в домен
            utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
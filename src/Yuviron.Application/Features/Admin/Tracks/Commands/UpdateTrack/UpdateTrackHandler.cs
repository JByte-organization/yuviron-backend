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
        // 1. Ищем трек вместе со связями
        var track = await _context.Tracks
                        .Include(t => t.TrackArtists)
                        .Include(t => t.TrackGenres)
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        // 2. Проверяем существование альбома (если передан)
        if (request.AlbumId.HasValue)
        {
            var albumExists = await _context.Albums
                .AsNoTracking()
                .AnyAsync(a => a.Id == request.AlbumId.Value, cancellationToken);

            if (!albumExists)
            {
                throw new NotFoundException(nameof(Album), request.AlbumId.Value);
            }
        }

        // 3. Проверяем существование артистов
        var uniqueArtistIds = request.ArtistIds.Distinct().ToList();
        var existingArtists = await _context.Artists.CountAsync(a => uniqueArtistIds.Contains(a.Id), cancellationToken);
        if (existingArtists != uniqueArtistIds.Count) 
        {
            throw new ArgumentException("Invalid artists provided.");
        }

        // 4. Проверяем существование жанров
        var uniqueGenreIds = request.GenreIds.Distinct().ToList();
        var existingGenres = await _context.Genres.CountAsync(g => uniqueGenreIds.Contains(g.Id), cancellationToken);
        if (existingGenres != uniqueGenreIds.Count) 
        {
            throw new ArgumentException("Invalid genres provided.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // 5. Обновляем доменную сущность
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
            utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
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

public sealed class UpdateTrackHandler : IRequestHandler<UpdateTrackCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateTrackHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateTrackCommand request, CancellationToken cancellationToken)
    {
        var track = await _context.Tracks
                        .Include(t => t.TrackArtists)
                        .Include(t => t.TrackGenres)
                        .Include(t => t.TrackMoods) 
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        // 2. Валидация альбома
        var albumExists = await _context.Albums
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.AlbumId, cancellationToken);
        if (!albumExists) throw new NotFoundException(nameof(Album), request.AlbumId);

        // 3. Валидация артистов
        var uniqueArtistIds = request.ArtistIds.Distinct().ToList();
        var existingArtists = await _context.Artists
            .AsNoTracking()
            .CountAsync(a => uniqueArtistIds.Contains(a.Id), cancellationToken);
            
        if (existingArtists != uniqueArtistIds.Count) throw new NotFoundException(nameof(Artist), "Invalid artists provided.");
        // 4. Валидация жанров
        var uniqueGenreIds = request.GenreIds.Distinct().ToList();
        var existingGenres = await _context.Genres
            .AsNoTracking()
            .CountAsync(g => uniqueGenreIds.Contains(g.Id), cancellationToken);
            
        if (existingGenres != uniqueGenreIds.Count) throw new NotFoundException(nameof(Genre), "Invalid genres provided.");

        // 5. Валидация настроений
        var uniqueMoodIds = request.MoodIds.Distinct().ToList();
        var existingMoods = await _context.Moods
            .AsNoTracking()
            .CountAsync(m => uniqueMoodIds.Contains(m.Id), cancellationToken);
            
        if (existingMoods != uniqueMoodIds.Count) throw new NotFoundException(nameof(Mood), "Invalid moods provided.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        track.UpdateDetails(
            request.AlbumId,
            request.AlbumPosition,
            request.Title,
            request.DurationMs,
            request.Explicit,
            request.CoverUrl,
            request.AudioStorageKey,
            request.PreviewStorageKey,
            request.VisibilityStatus,
            uniqueArtistIds,
            uniqueGenreIds,
            uniqueMoodIds, 
            utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
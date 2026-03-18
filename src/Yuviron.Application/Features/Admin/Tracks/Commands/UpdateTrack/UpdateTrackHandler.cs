using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrack;

public sealed class UpdateTrackHandler : IRequestHandler<UpdateTrackCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService;

    public UpdateTrackHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(UpdateTrackCommand request, CancellationToken cancellationToken)
    {
        var track = await _context.Tracks
                        .Include(t => t.TrackArtists)
                        .Include(t => t.TrackGenres)
                        .Include(t => t.TrackMoods) 
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var albumExists = await _context.Albums.AsNoTracking().AnyAsync(a => a.Id == request.AlbumId, cancellationToken);
        if (!albumExists) throw new NotFoundException(nameof(Album), request.AlbumId);

        var uniqueArtistIds = request.ArtistIds.Distinct().ToList();
        var existingArtists = await _context.Artists.AsNoTracking().CountAsync(a => uniqueArtistIds.Contains(a.Id), cancellationToken);
        if (existingArtists != uniqueArtistIds.Count) throw new NotFoundException(nameof(Artist), "Invalid artists provided.");
            
        var uniqueGenreIds = request.GenreIds.Distinct().ToList();
        var existingGenres = await _context.Genres.AsNoTracking().CountAsync(g => uniqueGenreIds.Contains(g.Id), cancellationToken);
        if (existingGenres != uniqueGenreIds.Count) throw new NotFoundException(nameof(Genre), "Invalid genres provided.");

        var uniqueMoodIds = request.MoodIds.Distinct().ToList();
        var existingMoods = await _context.Moods.AsNoTracking().CountAsync(m => uniqueMoodIds.Contains(m.Id), cancellationToken);
        if (existingMoods != uniqueMoodIds.Count) throw new NotFoundException(nameof(Mood), "Invalid moods provided.");

        var oldCoverUrl = track.CoverUrl;
        var oldAudioKey = track.AudioStorageKey;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var finalCoverUrl = await _fileStorageService.MoveIfTempAsync(request.CoverUrl, "covers", cancellationToken);
        var finalAudioKey = await _fileStorageService.MoveIfTempAsync(request.AudioStorageKey, "tracks", cancellationToken);

        track.UpdateDetails(
            request.AlbumId,
            request.AlbumPosition,
            request.Title,
            request.DurationMs,
            request.Explicit,
            finalCoverUrl,   
            finalAudioKey!, 
            request.VisibilityStatus,
            uniqueArtistIds,
            uniqueGenreIds,
            uniqueMoodIds, 
            utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.Equals(oldCoverUrl, finalCoverUrl, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(oldCoverUrl))
            await _fileStorageService.DeleteAsync(oldCoverUrl, cancellationToken);

        if (!string.Equals(oldAudioKey, finalAudioKey, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(oldAudioKey))
            await _fileStorageService.DeleteAsync(oldAudioKey, cancellationToken);


        return Unit.Value;
    }
}
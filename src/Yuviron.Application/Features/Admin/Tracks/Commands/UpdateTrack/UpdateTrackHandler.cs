using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrack;

public sealed class UpdateTrackHandler : IRequestHandler<UpdateTrackCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IAudioMetadataService _audioMetadataService;

    public UpdateTrackHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IAudioMetadataService audioMetadataService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _audioMetadataService = audioMetadataService;
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

        int updatedDurationMs = track.DurationMs;
        bool audioFileChanged = !string.Equals(oldAudioKey, request.AudioStorageKey, StringComparison.OrdinalIgnoreCase) 
                                && request.AudioStorageKey.StartsWith("temp/");

        if (audioFileChanged)
        {
            var audioMeta = await _audioMetadataService.GetAudioMetadataAsync(request.AudioStorageKey, cancellationToken);
            
            if (audioMeta.DurationMs < 1000 || audioMeta.DurationMs > 1000 * 60 * 60 * 3)
            {
                throw new InvalidOperationException("Audio track duration is out of allowed bounds.");
            }
            updatedDurationMs = audioMeta.DurationMs;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var finalCoverUrl = FileStorageExtensions.PredictDestinationPath(request.CoverUrl, "covers");

        track.UpdateDetails(
            request.AlbumId,
            request.AlbumPosition,
            request.Title,
            updatedDurationMs,
            request.Explicit,
            finalCoverUrl,   
            request.AudioStorageKey,
            request.VisibilityStatus,
            request.Isrc,
            uniqueArtistIds,
            uniqueGenreIds,
            uniqueMoodIds, 
            utcNow);

        if (!string.IsNullOrWhiteSpace(request.CoverUrl) && request.CoverUrl.StartsWith("temp/"))
            track.AddDomainEvent(new TempFileNeedsMovingEvent(request.CoverUrl, "covers"));

        if (!string.Equals(oldCoverUrl, finalCoverUrl, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(oldCoverUrl))
            track.AddDomainEvent(new FileNeedsDeletionEvent(oldCoverUrl));

        if (audioFileChanged)
        {
            track.AddDomainEvent(new AudioNeedsTranscodingEvent(track.Id, request.AudioStorageKey));
            
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
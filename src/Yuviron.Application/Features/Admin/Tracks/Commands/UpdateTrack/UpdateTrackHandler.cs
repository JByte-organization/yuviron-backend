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
    private readonly ICurrentUserService _currentUser;

    public UpdateTrackHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IAudioMetadataService audioMetadataService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _audioMetadataService = audioMetadataService;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateTrackCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var track = await _context.Tracks
            .Include(t => t.TrackArtists)
            .Include(t => t.TrackGenres)
            .Include(t => t.TrackMoods) 
            .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken);

        if (track == null)
        {
            throw new NotFoundException(nameof(Track), request.TrackId);
        }

        var albumExists = await _context.Albums.AsNoTracking().AnyAsync(a => a.Id == request.AlbumId, cancellationToken);
        if (!albumExists)
        {
            throw new NotFoundException(nameof(Album), request.AlbumId);
        }

        var isPositionTaken = await _context.Tracks.AnyAsync(t => t.AlbumId == request.AlbumId && t.AlbumPosition == request.AlbumPosition && t.Id != request.TrackId, cancellationToken);
        if (isPositionTaken)
        {
            throw new PositionConflictException(request.AlbumPosition, "Track in this Album");
        }
        
        var uniqueArtists = request.Artists.DistinctBy(a => a.Id).ToList();
        var uniqueArtistIds = uniqueArtists.Select(a => a.Id).ToList();
        var existingArtists = await _context.Artists.AsNoTracking().CountAsync(a => uniqueArtistIds.Contains(a.Id), cancellationToken);
        if (existingArtists != uniqueArtistIds.Count)
        {
            throw new NotFoundException(nameof(Artist), "Invalid artists provided.");
        }
            
        var uniqueGenreIds = request.GenreIds.Distinct().ToList();
        var existingGenres = await _context.Genres.AsNoTracking().CountAsync(g => uniqueGenreIds.Contains(g.Id), cancellationToken);
        if (existingGenres != uniqueGenreIds.Count)
        {
            throw new NotFoundException(nameof(Genre), "Invalid genres provided.");
        }

        var uniqueMoodIds = request.MoodIds.Distinct().ToList();
        var existingMoods = await _context.Moods.AsNoTracking().CountAsync(m => uniqueMoodIds.Contains(m.Id), cancellationToken);
        if (existingMoods != uniqueMoodIds.Count)
        {
            throw new NotFoundException(nameof(Mood), "Invalid moods provided.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        int updatedDurationMs = track.DurationMs;
        string finalAudioKey = track.AudioStorageKey;
        bool audioFileChanged = false;

        if (request.AudioFileId.HasValue)
        {
            audioFileChanged = true;
            var oldAudioKey = track.AudioStorageKey;
            var targetAudioFolder = $"tracks/{track.Id}";

            var audioClaim = await _context.ClaimFileAsync(
                request.AudioFileId.Value, adminId, "audio/", targetAudioFolder, cancellationToken);
            
            var audioMeta = await _audioMetadataService.GetAudioMetadataAsync(audioClaim.SourceKey, cancellationToken);
            
            if (audioMeta.DurationMs < 1000 || audioMeta.DurationMs > 1000 * 60 * 60 * 3)
            {
                throw new InvalidOperationException("Audio track duration is out of allowed bounds.");
            }

            if (!string.IsNullOrWhiteSpace(oldAudioKey)
                && !string.Equals(oldAudioKey, audioClaim.SourceKey, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(oldAudioKey, audioClaim.FinalPath, StringComparison.OrdinalIgnoreCase))
            {
                track.AddDomainEvent(new FileNeedsDeletionEvent(oldAudioKey));
            }

            track.AddDomainEvent(new AudioNeedsTranscodingEvent(track.Id, audioClaim.SourceKey));
            
            updatedDurationMs = audioMeta.DurationMs;
            finalAudioKey = audioClaim.SourceKey;
        }

        string? finalCoverUrl = track.CoverUrl;
        if (request.CoverFileId.HasValue)
        {
            var coverClaim = await _context.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
            
            track.RegisterFileSwapEvents(coverClaim, track.CoverUrl);
            finalCoverUrl = coverClaim.FinalPath;
        }

        track.UpdateDetails(
            request.AlbumId,
            request.AlbumPosition,
            request.Title,
            updatedDurationMs,
            request.Explicit,
            finalCoverUrl,   
            finalAudioKey,
            request.VisibilityStatus,
            request.Isrc,
            uniqueArtists.Select(a => (a.Id, a.Role)),
            uniqueGenreIds,
            uniqueMoodIds, 
            utcNow, 
            audioFileChanged);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
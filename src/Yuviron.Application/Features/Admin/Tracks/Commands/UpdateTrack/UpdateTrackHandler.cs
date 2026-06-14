using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly IAudioMetadataService _audioMetadataService;
    private readonly ICurrentUserService _currentUser;

    public UpdateTrackHandler(
        ICatalogContext catalogContext, ISystemContext systemContext, 
        TimeProvider timeProvider,
        IAudioMetadataService audioMetadataService,
        ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _audioMetadataService = audioMetadataService;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateTrackCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var track = await _catalogContext.Tracks
            .Include(t => t.TrackArtists)
            .Include(t => t.TrackGenres)
            .Include(t => t.TrackMoods) 
            .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
            ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var albumExists = await _catalogContext.Albums.AsNoTracking().AnyAsync(a => a.Id == request.AlbumId, cancellationToken);
        if (!albumExists) throw new NotFoundException(nameof(Album), request.AlbumId);

        var isPositionTaken = await _catalogContext.Tracks.AnyAsync(t => t.AlbumId == request.AlbumId && t.AlbumPosition == request.AlbumPosition && t.Id != request.TrackId, cancellationToken);
        if (isPositionTaken) throw new PositionConflictException(request.AlbumPosition, "Track in this Album");
        
        var uniqueArtists = request.Artists.DistinctBy(a => a.Id).ToList();
        var uniqueArtistIds = uniqueArtists.Select(a => a.Id).ToList();
        var uniqueGenreIds = request.GenreIds.Distinct().ToList();
        var uniqueMoodIds = request.MoodIds.Distinct().ToList();

        await _catalogContext.Artists.EnsureAllExistAsync(uniqueArtistIds, nameof(Artist), cancellationToken);
        await _catalogContext.Genres.EnsureAllExistAsync(uniqueGenreIds, nameof(Genre), cancellationToken);
        await _catalogContext.Moods.EnsureAllExistAsync(uniqueMoodIds, nameof(Mood), cancellationToken);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        int updatedDurationMs = track.DurationMs;
        string finalAudioKey = track.AudioStorageKey;
        bool audioFileChanged = false;

        if (request.AudioFileId.HasValue)
        {
            audioFileChanged = true;
            var oldAudioKey = track.AudioStorageKey;
            var targetAudioFolder = $"tracks/{track.Id}";

            var audioClaim = await _systemContext.ClaimFileAsync(
                request.AudioFileId.Value, adminId, "audio/", targetAudioFolder, cancellationToken);
            
            var audioMeta = await _audioMetadataService.GetAudioMetadataAsync(audioClaim.SourceKey, cancellationToken);
            
            if (audioMeta.DurationMs < 1000 || audioMeta.DurationMs > 1000 * 60 * 60 * 3)
                throw new InvalidOperationException("Audio track duration is out of allowed bounds.");

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
            var coverClaim = await _systemContext.ClaimFileAsync(
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

        await _catalogContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
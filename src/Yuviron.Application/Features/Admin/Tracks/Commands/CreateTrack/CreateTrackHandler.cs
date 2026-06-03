using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;

public sealed class CreateTrackHandler : IRequestHandler<CreateTrackCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IAudioMetadataService _audioMetadataService;
    private readonly ICurrentUserService _currentUser;

    public CreateTrackHandler(
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

    public async Task<Guid> Handle(CreateTrackCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var albumExists = await _context.Albums.AnyAsync(a => a.Id == request.AlbumId, cancellationToken);
        if (!albumExists) throw new NotFoundException(nameof(Album), request.AlbumId);

        var isPositionTaken = await _context.Tracks.AnyAsync(t => t.AlbumId == request.AlbumId && t.AlbumPosition == request.AlbumPosition, cancellationToken);
        if (isPositionTaken) throw new PositionConflictException(request.AlbumPosition, "Track in this Album");
        
        var uniqueArtists = request.Artists.DistinctBy(a => a.Id).ToList();
        var uniqueArtistIds = uniqueArtists.Select(a => a.Id).ToList();
        var uniqueGenreIds = request.GenreIds.Distinct().ToList();
        var uniqueMoodIds = request.MoodIds.Distinct().ToList();

        await _context.Artists.EnsureAllExistAsync(uniqueArtistIds, nameof(Artist), cancellationToken);
        await _context.Genres.EnsureAllExistAsync(uniqueGenreIds, nameof(Genre), cancellationToken);
        await _context.Moods.EnsureAllExistAsync(uniqueMoodIds, nameof(Mood), cancellationToken);

        var trackId = Guid.NewGuid();
        var targetAudioFolder = $"tracks/{trackId}";

        var audioClaim = await _context.ClaimFileAsync(
            request.AudioFileId, adminId, "audio/", targetAudioFolder, cancellationToken);

        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _context.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
        }

        var audioMeta = await _audioMetadataService.GetAudioMetadataAsync(audioClaim.SourceKey, cancellationToken);
        if (audioMeta.DurationMs < 1000 || audioMeta.DurationMs > 1000 * 60 * 60 * 3)
            throw new InvalidOperationException("Audio track duration is out of allowed bounds.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var track = Track.Create(
            trackId, 
            request.AlbumId,
            request.AlbumPosition,
            request.Title,
            audioMeta.DurationMs,
            request.Explicit,
            coverClaim?.FinalPath,   
            audioClaim.SourceKey,
            request.VisibilityStatus,
            request.Isrc,
            uniqueArtists.Select(a => (a.Id, a.Role)),
            uniqueGenreIds,
            uniqueMoodIds, 
            utcNow);

        if (coverClaim != null)
        {
            track.RegisterFileSwapEvents(coverClaim);
        }

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync(cancellationToken);

        return track.Id;
    }
}
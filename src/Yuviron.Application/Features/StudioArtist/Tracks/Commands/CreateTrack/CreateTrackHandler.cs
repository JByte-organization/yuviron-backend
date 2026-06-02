using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;

public sealed class CreateTrackHandler : IRequestHandler<CreateTrackCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IAudioMetadataService _audioMetadataService;

    public CreateTrackHandler(
        IApplicationDbContext context, TimeProvider timeProvider,
        ICurrentUserService currentUser, IAudioMetadataService audioMetadataService)
    {
        _context = context; _timeProvider = timeProvider;
        _currentUser = currentUser; _audioMetadataService = audioMetadataService;
    }

    public async Task<Guid> Handle(CreateTrackCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var album = await _context.Albums.Include(a => a.AlbumArtists)
            .FirstOrDefaultAsync(a => a.Id == request.AlbumId, cancellationToken)
            ?? throw new NotFoundException(nameof(Album), request.AlbumId);

        var hasAccess = await _context.ArtistTeamMembers
            .HasManagementAccess(album.AlbumArtists.Select(aa => aa.ArtistId), userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this album.");

        var uniqueGenres = request.GenreIds?.Distinct().ToList() ?? new List<Guid>();
        var uniqueMoods = request.MoodIds?.Distinct().ToList() ?? new List<Guid>();
        var collaboratorIds = request.Collaborators?.Select(c => c.ArtistId).Distinct().ToList() ?? new List<Guid>();

        await _context.Genres.EnsureAllExistAsync(uniqueGenres, nameof(Genre), cancellationToken);
        await _context.Moods.EnsureAllExistAsync(uniqueMoods, nameof(Mood), cancellationToken);
        await _context.Artists.EnsureAllExistAsync(collaboratorIds, nameof(Artist), cancellationToken);

        int position = request.Position ?? await _context.Tracks.CountAsync(t => t.AlbumId == album.Id, cancellationToken) + 1;

        var isPositionTaken = await _context.Tracks.AnyAsync(t => t.AlbumId == album.Id && t.AlbumPosition == position, cancellationToken);
        if (isPositionTaken) throw new PositionConflictException(position, "Track in this Album");

        var trackId = Guid.NewGuid();
        var audioClaim = await _context.ClaimFileAsync(request.AudioFileId, userId, "audio/", $"tracks/{trackId}", cancellationToken);

        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _context.ClaimFileAsync(request.CoverFileId.Value, userId, "image/", "tracks/covers", cancellationToken);
        }

        var audioMeta = await _audioMetadataService.GetAudioMetadataAsync(audioClaim.SourceKey, cancellationToken);
        if (audioMeta.DurationMs < 1000 || audioMeta.DurationMs > 1000 * 60 * 60 * 3) throw new InvalidOperationException("Invalid duration.");

        var trackArtists = request.Collaborators?.Select(c => (c.ArtistId, c.Role)).ToList();
        if (trackArtists == null || !trackArtists.Any()) trackArtists = album.AlbumArtists.Select(aa => (aa.ArtistId, aa.Role)).ToList();

        var track = Track.Create(
            trackId, album.Id, position, request.Title, audioMeta.DurationMs, request.Explicit,
            coverClaim?.FinalPath, audioClaim.SourceKey, album.VisibilityStatus, null,
            trackArtists, uniqueGenres, uniqueMoods, _timeProvider.GetUtcNow().UtcDateTime);

        track.RegisterFileSwapEvents(audioClaim);
        if (coverClaim != null) track.RegisterFileSwapEvents(coverClaim);

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync(cancellationToken);

        return track.Id;
    }
}
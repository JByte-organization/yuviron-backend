using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;

public sealed class CreateTrackHandler : IRequestHandler<CreateTrackCommand, Guid>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IAudioMetadataService _audioMetadataService;

    public CreateTrackHandler(
        ICatalogContext catalogContext, ISystemContext systemContext, TimeProvider timeProvider,
        ICurrentUserService currentUser, IAudioMetadataService audioMetadataService)
    {
        _catalogContext = catalogContext;
        _systemContext = systemContext; _timeProvider = timeProvider;
        _currentUser = currentUser; _audioMetadataService = audioMetadataService;
    }

    public async Task<Guid> Handle(CreateTrackCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var album = await _catalogContext.Albums.Include(a => a.AlbumArtists)
            .FirstOrDefaultAsync(a => a.Id == request.AlbumId, cancellationToken)
            ?? throw new NotFoundException(nameof(Album), request.AlbumId);

        // Only Main artists of the album can add tracks
        var mainArtistIds = album.AlbumArtists
            .Where(aa => aa.Role == ArtistRole.Main)
            .Select(aa => aa.ArtistId)
            .ToList();

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .HasManagementAccess(mainArtistIds, userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this album (only Main artists or managers).");

        var uniqueGenres = request.GenreIds?.Distinct().ToList() ?? new List<Guid>();
        var uniqueMoods = request.MoodIds?.Distinct().ToList() ?? new List<Guid>();
        var collaboratorIds = request.Collaborators?.Select(c => c.ArtistId).Distinct().ToList() ?? new List<Guid>();

        await _catalogContext.Genres.EnsureAllExistAsync(uniqueGenres, nameof(Genre), cancellationToken);
        await _catalogContext.Moods.EnsureAllExistAsync(uniqueMoods, nameof(Mood), cancellationToken);
        await _catalogContext.Artists.EnsureAllExistAsync(collaboratorIds, nameof(Artist), cancellationToken);

        int position = request.Position ?? await _catalogContext.Tracks.CountAsync(t => t.AlbumId == album.Id, cancellationToken) + 1;

        var isPositionTaken = await _catalogContext.Tracks.AnyAsync(t => t.AlbumId == album.Id && t.AlbumPosition == position, cancellationToken);
        if (isPositionTaken) throw new PositionConflictException(position, "Track in this Album");

        var trackId = Guid.NewGuid();
        var audioClaim = await _systemContext.ClaimFileAsync(request.AudioFileId, userId, "audio/", $"tracks/{trackId}", cancellationToken);

        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _systemContext.ClaimFileAsync(request.CoverFileId.Value, userId, "image/", "covers", cancellationToken);
        }

        var audioMeta = await _audioMetadataService.GetAudioMetadataAsync(audioClaim.SourceKey, cancellationToken);
        if (audioMeta.DurationMs < 1000 || audioMeta.DurationMs > 1000 * 60 * 60 * 3) throw new InvalidOperationException("Invalid duration.");

        var trackArtists = request.Collaborators?.Select(c => (c.ArtistId, c.Role)).ToList();
        if (trackArtists == null || !trackArtists.Any()) trackArtists = album.AlbumArtists.Select(aa => (aa.ArtistId, aa.Role)).ToList();

        var track = Track.Create(
            trackId, album.Id, position, request.Title, audioMeta.DurationMs, request.Explicit,
            coverClaim?.FinalPath, audioClaim.SourceKey, album.VisibilityStatus, null,
            trackArtists, uniqueGenres, uniqueMoods, _timeProvider.GetUtcNow().UtcDateTime);

        
        if (coverClaim != null) 
        {
            track.RegisterFileSwapEvents(coverClaim);
        }

        _catalogContext.Add(track);
        await _catalogContext.SaveChangesAsync(cancellationToken);

        return track.Id;
    }
}

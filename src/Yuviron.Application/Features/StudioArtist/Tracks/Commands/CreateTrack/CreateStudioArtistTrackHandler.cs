using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;

public sealed class CreateStudioArtistTrackHandler : IRequestHandler<CreateStudioArtistTrackCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IAudioMetadataService _audioMetadataService;
    private readonly ICurrentUserService _currentUser;

    public CreateStudioArtistTrackHandler(
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

    public async Task<Guid> Handle(CreateStudioArtistTrackCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var currentArtistId = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.UserId == userId)
            .OrderBy(tm => tm.Role == ArtistTeamRole.Owner ? 0 :
                tm.Role == ArtistTeamRole.Manager ? 1 :
                tm.Role == ArtistTeamRole.Editor ? 2 :
                tm.Role == ArtistTeamRole.Viewer ? 3 : 4)
            .ThenByDescending(tm => tm.CreatedAt)
            .Select(tm => (Guid?)tm.ArtistId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!currentArtistId.HasValue)
        {
            throw new NotFoundException(nameof(Artist), $"for user {userId}");
        }

        var albumProjection = await _context.Albums
            .AsNoTracking()
            .Where(a => a.Id == request.AlbumId)
            .Select(a => new
            {
                a.Id,
                HasCurrentArtist = a.AlbumArtists.Any(aa => aa.ArtistId == currentArtistId.Value)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (albumProjection is null)
        {
            throw new NotFoundException(nameof(Album), request.AlbumId);
        }

        if (!albumProjection.HasCurrentArtist)
        {
            throw new ForbiddenException("You can only add tracks to albums linked to your current artist profile.");
        }

        var uniqueCoAuthorIds = (request.CoAuthorIds ?? [])
            .Where(id => id != Guid.Empty && id != currentArtistId.Value)
            .Distinct()
            .ToList();

        if (uniqueCoAuthorIds.Count > 0)
        {
            var existingCoAuthorsCount = await _context.Artists
                .AsNoTracking()
                .CountAsync(a => uniqueCoAuthorIds.Contains(a.Id), cancellationToken);

            if (existingCoAuthorsCount != uniqueCoAuthorIds.Count)
            {
                throw new NotFoundException(nameof(Artist), "One or more provided IDs");
            }
        }

        var nextAlbumPosition = (await _context.Tracks
            .AsNoTracking()
            .Where(t => t.AlbumId == request.AlbumId)
            .MaxAsync(t => (int?)t.AlbumPosition, cancellationToken) ?? 0) + 1;

        var trackId = Guid.NewGuid();
        var targetAudioFolder = $"tracks/{trackId}";

        var audioClaim = await _context.ClaimFileAsync(
            request.AudioFileId,
            userId,
            "audio/",
            targetAudioFolder,
            cancellationToken);

        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _context.ClaimFileAsync(
                request.CoverFileId.Value,
                userId,
                "image/",
                "covers",
                cancellationToken);
        }

        var audioMeta = await _audioMetadataService.GetAudioMetadataAsync(audioClaim.SourceKey, cancellationToken);

        if (audioMeta.DurationMs < 1000 || audioMeta.DurationMs > 1000 * 60 * 60 * 3)
        {
            throw new InvalidOperationException("Audio track duration is out of allowed bounds.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var artists = new List<(Guid ArtistId, ArtistRole Role)>
        {
            (currentArtistId.Value, ArtistRole.Main)
        };

        artists.AddRange(uniqueCoAuthorIds.Select(id => (id, ArtistRole.Feat)));

        var track = Track.Create(
            trackId,
            request.AlbumId,
            nextAlbumPosition,
            request.Title,
            audioMeta.DurationMs,
            request.Explicit,
            coverClaim?.FinalPath,
            audioClaim.SourceKey,
            VisibilityStatus.Draft,
            null,
            artists,
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
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

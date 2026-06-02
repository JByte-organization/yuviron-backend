using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateTrack;

public sealed class UpdateStudioArtistTrackHandler : IRequestHandler<UpdateStudioArtistTrackCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public UpdateStudioArtistTrackHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateStudioArtistTrackCommand request, CancellationToken cancellationToken)
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

        var track = await _context.Tracks
            .Include(t => t.TrackArtists)
            .Include(t => t.TrackGenres)
            .Include(t => t.TrackMoods)
            .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken);

        if (track is null)
        {
            throw new NotFoundException(nameof(Track), request.TrackId);
        }

        var belongsToCurrentArtist = await _context.AlbumArtists
            .AsNoTracking()
            .AnyAsync(aa => aa.AlbumId == track.AlbumId && aa.ArtistId == currentArtistId.Value, cancellationToken);

        if (!belongsToCurrentArtist)
        {
            throw new ForbiddenException("You can only edit tracks linked to albums of your current artist profile.");
        }

        var existingCoAuthors = track.TrackArtists
            .Where(ta => ta.ArtistId != currentArtistId.Value)
            .Select(ta => (ta.ArtistId, ta.Role))
            .ToList();

        var artists = new List<(Guid ArtistId, ArtistRole Role)>
        {
            (currentArtistId.Value, ArtistRole.Main)
        };

        if (request.CoAuthorIds is null)
        {
            artists.AddRange(existingCoAuthors);
        }
        else
        {
            var uniqueCoAuthorIds = request.CoAuthorIds
                .Where(id => id != Guid.Empty && id != currentArtistId.Value)
                .Distinct()
                .ToList();

            if (uniqueCoAuthorIds.Count > 0)
            {
                var existingArtistsCount = await _context.Artists
                    .AsNoTracking()
                    .CountAsync(a => uniqueCoAuthorIds.Contains(a.Id), cancellationToken);

                if (existingArtistsCount != uniqueCoAuthorIds.Count)
                {
                    throw new NotFoundException(nameof(Artist), "One or more provided IDs");
                }
            }

            artists.AddRange(uniqueCoAuthorIds.Select(id => (id, ArtistRole.Feat)));
        }

        var finalCoverUrl = track.CoverUrl;
        if (request.CoverFileId.HasValue)
        {
            var coverClaim = await _context.ClaimFileAsync(
                request.CoverFileId.Value,
                userId,
                "image/",
                "covers",
                cancellationToken);

            track.RegisterFileSwapEvents(coverClaim, track.CoverUrl);
            finalCoverUrl = coverClaim.FinalPath;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        track.UpdateDetails(
            track.AlbumId,
            track.AlbumPosition,
            request.Title,
            track.DurationMs,
            track.Explicit,
            finalCoverUrl,
            track.AudioStorageKey,
            track.VisibilityStatus,
            track.Isrc,
            artists,
            track.TrackGenres.Select(tg => tg.GenreId).ToList(),
            track.TrackMoods.Select(tm => tm.MoodId).ToList(),
            utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

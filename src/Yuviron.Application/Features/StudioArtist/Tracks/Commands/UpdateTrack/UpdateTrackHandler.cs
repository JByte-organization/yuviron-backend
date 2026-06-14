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

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateTrack;

public sealed class UpdateTrackHandler : IRequestHandler<UpdateTrackCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdateTrackHandler(ICatalogContext catalogContext, ISystemContext systemContext, TimeProvider timeProvider, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _systemContext = systemContext; _timeProvider = timeProvider; _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateTrackCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var track = await _catalogContext.Tracks
            .Include(t => t.Album).ThenInclude(a => a!.AlbumArtists)
            .Include(t => t.TrackArtists).Include(t => t.TrackGenres).Include(t => t.TrackMoods)
            .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
            ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .HasManagementAccess(track.Album!.AlbumArtists.Select(aa => aa.ArtistId), userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this track.");

        var position = request.Position ?? track.AlbumPosition;
        if (position != track.AlbumPosition)
        {
            var isPosTaken = await _catalogContext.Tracks.AnyAsync(t => t.AlbumId == track.AlbumId && t.AlbumPosition == position && t.Id != request.TrackId, cancellationToken);
            if (isPosTaken) throw new PositionConflictException(position, "Track in this Album");
        }

        var uniqueGenres = request.GenreIds?.Distinct().ToList() ?? track.TrackGenres.Select(tg => tg.GenreId).ToList();
        var uniqueMoods = request.MoodIds?.Distinct().ToList() ?? track.TrackMoods.Select(tm => tm.MoodId).ToList();
        var trackArtists = request.Collaborators?.Select(c => (c.ArtistId, c.Role)).ToList() ?? track.TrackArtists.Select(ta => (ta.ArtistId, ta.Role)).ToList();

        await _catalogContext.Genres.EnsureAllExistAsync(uniqueGenres, nameof(Genre), cancellationToken);
        await _catalogContext.Moods.EnsureAllExistAsync(uniqueMoods, nameof(Mood), cancellationToken);
        await _catalogContext.Artists.EnsureAllExistAsync(trackArtists.Select(a => a.ArtistId), nameof(Artist), cancellationToken);

        string? finalCoverUrl = track.CoverUrl;
        if (request.CoverFileId.HasValue)
        {
            var coverClaim = await _systemContext.ClaimFileAsync(request.CoverFileId.Value, userId, "image/", "covers", cancellationToken);
            track.RegisterFileSwapEvents(coverClaim, track.CoverUrl);
            finalCoverUrl = coverClaim.FinalPath;
        }

        track.UpdateDetails(
            track.AlbumId, position, request.Title, track.DurationMs, request.Explicit,
            finalCoverUrl, track.AudioStorageKey, track.VisibilityStatus, track.Isrc,
            trackArtists, uniqueGenres, uniqueMoods, _timeProvider.GetUtcNow().UtcDateTime, audioChanged: false);

        await _catalogContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
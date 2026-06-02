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
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdateTrackHandler(IApplicationDbContext context, TimeProvider timeProvider, ICurrentUserService currentUser)
    {
        _context = context; _timeProvider = timeProvider; _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateTrackCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var track = await _context.Tracks
            .Include(t => t.Album).ThenInclude(a => a!.AlbumArtists)
            .Include(t => t.TrackArtists).Include(t => t.TrackGenres).Include(t => t.TrackMoods)
            .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
            ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var hasAccess = await _context.ArtistTeamMembers
            .HasManagementAccess(track.Album!.AlbumArtists.Select(aa => aa.ArtistId), userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this track.");

        var position = request.Position ?? track.AlbumPosition;
        if (position != track.AlbumPosition)
        {
            var isPosTaken = await _context.Tracks.AnyAsync(t => t.AlbumId == track.AlbumId && t.AlbumPosition == position && t.Id != request.TrackId, cancellationToken);
            if (isPosTaken) throw new PositionConflictException(position, "Track in this Album");
        }

        // ЕЛЕГАНТНА ПЕРЕВІРКА ЗВ'ЯЗКІВ
        var uniqueGenres = request.GenreIds?.Distinct().ToList() ?? track.TrackGenres.Select(tg => tg.GenreId).ToList();
        var uniqueMoods = request.MoodIds?.Distinct().ToList() ?? track.TrackMoods.Select(tm => tm.MoodId).ToList();
        var trackArtists = request.Collaborators?.Select(c => (c.ArtistId, c.Role)).ToList() ?? track.TrackArtists.Select(ta => (ta.ArtistId, ta.Role)).ToList();

        await _context.Genres.EnsureAllExistAsync(uniqueGenres, nameof(Genre), cancellationToken);
        await _context.Moods.EnsureAllExistAsync(uniqueMoods, nameof(Mood), cancellationToken);
        await _context.Artists.EnsureAllExistAsync(trackArtists.Select(a => a.ArtistId), nameof(Artist), cancellationToken);

        string? finalCoverUrl = track.CoverUrl;
        if (request.CoverFileId.HasValue)
        {
            var coverClaim = await _context.ClaimFileAsync(request.CoverFileId.Value, userId, "image/", "tracks/covers", cancellationToken);
            track.RegisterFileSwapEvents(coverClaim, track.CoverUrl);
            finalCoverUrl = coverClaim.FinalPath;
        }

        track.UpdateDetails(
            track.AlbumId, position, request.Title, track.DurationMs, request.Explicit,
            finalCoverUrl, track.AudioStorageKey, track.VisibilityStatus, track.Isrc,
            trackArtists, uniqueGenres, uniqueMoods, _timeProvider.GetUtcNow().UtcDateTime, audioChanged: false);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
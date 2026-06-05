using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.ChangeTrackPositionInStudioPlaylist;

public sealed class ChangeTrackPositionInStudioPlaylistHandler : IRequestHandler<ChangeTrackPositionInStudioPlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public ChangeTrackPositionInStudioPlaylistHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider, ICacheService cache)
    {
        _context = context; _currentUser = currentUser; _timeProvider = timeProvider; _cache = cache;
    }

    public async Task<Unit> Handle(ChangeTrackPositionInStudioPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
            ?? throw new NotFoundException("Playlist", request.PlaylistId);

        if (playlist.ArtistId == null) throw new ForbiddenException("This is not a studio artist playlist.");

        var hasPermission = await _context.ArtistTeamMembers
            .HasManagementAccess(playlist.ArtistId.Value, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to manage this playlist.");

        var trackToMove = await _context.PlaylistTracks
            .FirstOrDefaultAsync(t => t.PlaylistId == request.PlaylistId && t.TrackId == request.TrackId, cancellationToken)
            ?? throw new NotFoundException("PlaylistTrack", request.TrackId);

        trackToMove.UpdatePosition(request.NewPosition);
        playlist.NotifyContentChanged(_timeProvider.GetUtcNow().UtcDateTime);

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.SortedSetAddAsync($"playlist:{request.PlaylistId}:tracks", request.TrackId.ToString(), request.NewPosition, cancellationToken);

        return Unit.Value;
    }
}
using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.ChangeTrackPositionInStudioPlaylist;

public sealed class ChangeTrackPositionInStudioPlaylistHandler : IRequestHandler<ChangeTrackPositionInStudioPlaylistCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;
    private readonly IEventBus _eventBus;

    public ChangeTrackPositionInStudioPlaylistHandler(
        ICatalogContext catalogContext, ILibraryContext libraryContext, ICurrentUserService currentUser, TimeProvider timeProvider, ICacheService cache, IEventBus eventBus)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext; _currentUser = currentUser; _timeProvider = timeProvider; _cache = cache; _eventBus = eventBus;
    }

    public async Task<Unit> Handle(ChangeTrackPositionInStudioPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _libraryContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
            ?? throw new NotFoundException("Playlist", request.PlaylistId);

        if (playlist.ArtistId == null) throw new ForbiddenException("This is not a studio artist playlist.");

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasManagementAccess(playlist.ArtistId.Value, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to manage this playlist.");

        var trackToMove = await _libraryContext.PlaylistTracks
            .Include(t => t.Track)
            .FirstOrDefaultAsync(t => t.PlaylistId == request.PlaylistId && t.TrackId == request.TrackId, cancellationToken)
            ?? throw new NotFoundException("PlaylistTrack", request.TrackId);

        trackToMove.UpdatePosition(request.NewPosition);
        playlist.NotifyContentChanged(_timeProvider.GetUtcNow().UtcDateTime);

        await _catalogContext.SaveChangesAsync(cancellationToken);

        await _cache.SortedSetAddAsync($"playlist:{request.PlaylistId}:tracks", request.TrackId.ToString(), request.NewPosition, cancellationToken);

        await _eventBus.PublishAsync(
            new StudioPlaylistTrackChangedEvent(
                playlist.ArtistId.Value,
                playlist.Id,
                playlist.Title,
                trackToMove.TrackId,
                trackToMove.Track.Title,
                userId,
                "position_changed"),
            cancellationToken);

        return Unit.Value;
    }
}

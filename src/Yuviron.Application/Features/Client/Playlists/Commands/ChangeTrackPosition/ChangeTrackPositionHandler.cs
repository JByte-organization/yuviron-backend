using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Playlists.Commands.ChangeTrackPosition;

public sealed class ChangeTrackPositionHandler : IRequestHandler<ChangeTrackPositionCommand>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public ChangeTrackPositionHandler(ILibraryContext libraryContext, ICurrentUserService currentUser, TimeProvider timeProvider, ICacheService cache)
    {
        _libraryContext = libraryContext;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _cache = cache;
    }

    public async Task Handle(ChangeTrackPositionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _libraryContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId , cancellationToken);

        if (playlist is null) throw new NotFoundException(nameof(Playlist), request.PlaylistId);
        if (playlist.UserId != userId) throw new ForbiddenException("Access denied.");

        var trackToMove = await _libraryContext.PlaylistTracks
            .FirstOrDefaultAsync(t => t.PlaylistId == request.PlaylistId && t.TrackId == request.TrackId, cancellationToken)
            ?? throw new NotFoundException("PlaylistTrack", request.TrackId);

        trackToMove.UpdatePosition(request.NewPosition);
        playlist.NotifyContentChanged(_timeProvider.GetUtcNow().UtcDateTime);

        await _libraryContext.SaveChangesAsync(cancellationToken);

        await _cache.SortedSetAddAsync($"playlist:{request.PlaylistId}:tracks", request.TrackId.ToString(), request.NewPosition, cancellationToken);
    }
}
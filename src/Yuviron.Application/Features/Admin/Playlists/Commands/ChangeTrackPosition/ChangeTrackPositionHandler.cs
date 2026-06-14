using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.ChangeTrackPosition;

public sealed class ChangeTrackPositionHandler : IRequestHandler<ChangeTrackPositionCommand, Unit>
{
    private readonly ILibraryContext _libraryContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public ChangeTrackPositionHandler(
        ILibraryContext libraryContext, 
        TimeProvider timeProvider,
        ICacheService cache)
    {
        _libraryContext = libraryContext;
        _timeProvider = timeProvider;
        _cache = cache;
    }

    public async Task<Unit> Handle(ChangeTrackPositionCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _libraryContext.Playlists
                           .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        var trackToMove = await _libraryContext.PlaylistTracks
                              .FirstOrDefaultAsync(t => t.PlaylistId == request.PlaylistId && t.TrackId == request.TrackId, cancellationToken)
                          ?? throw new NotFoundException("PlaylistTrack", request.TrackId);

        trackToMove.UpdatePosition(request.NewPosition);
        playlist.NotifyContentChanged(_timeProvider.GetUtcNow().UtcDateTime);

        await _libraryContext.SaveChangesAsync(cancellationToken);

        var redisKey = $"playlist:{request.PlaylistId}:tracks";
        await _cache.SortedSetAddAsync(redisKey, request.TrackId.ToString(), request.NewPosition, cancellationToken);

        return Unit.Value;
    }
}
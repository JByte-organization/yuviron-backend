using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.RemoveTrackFromPlaylist;

public sealed class RemoveTrackFromPlaylistHandler : IRequestHandler<RemoveTrackFromPlaylistCommand, Unit>
{
    private readonly ILibraryContext _libraryContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public RemoveTrackFromPlaylistHandler(
        ILibraryContext libraryContext, 
        TimeProvider timeProvider,
        ICacheService cache)
    {
        _libraryContext = libraryContext;
        _timeProvider = timeProvider;
        _cache = cache;
    }

    public async Task<Unit> Handle(RemoveTrackFromPlaylistCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _libraryContext.Playlists
                           .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        var deletedRows = await _libraryContext.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows > 0)
        {
            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            playlist.NotifyContentChanged(utcNow);
            await _libraryContext.SaveChangesAsync(cancellationToken);

            var redisKey = $"playlist:{request.PlaylistId}:tracks";
            await _cache.SortedSetRemoveAsync(redisKey, request.TrackId.ToString(), cancellationToken);
        }

        return Unit.Value;
    }
}
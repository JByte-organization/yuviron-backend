using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Playlists.Commands.RemoveTrackFromPlaylist;

public sealed class RemoveTrackFromPlaylistHandler : IRequestHandler<RemoveTrackFromPlaylistCommand>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public RemoveTrackFromPlaylistHandler(ILibraryContext libraryContext, ICurrentUserService currentUser, TimeProvider timeProvider, ICacheService cache)
    {
        _libraryContext = libraryContext;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _cache = cache;
    }

    public async Task Handle(RemoveTrackFromPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var isOwner = await _libraryContext.Playlists
            .AnyAsync(p => p.Id == request.PlaylistId && p.UserId == userId , cancellationToken);

        if (!isOwner) throw new ForbiddenException("Playlist not found or access denied.");

        var deletedRows = await _libraryContext.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId)
            .ExecuteDeleteAsync(cancellationToken);
            
        if (deletedRows > 0)
        {
            await _libraryContext.Playlists
                .Where(p => p.Id == request.PlaylistId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.UpdatedAt, _timeProvider.GetUtcNow().UtcDateTime), cancellationToken);

            await _cache.SortedSetRemoveAsync($"playlist:{request.PlaylistId}:tracks", request.TrackId.ToString(), cancellationToken);
        }
    }
}
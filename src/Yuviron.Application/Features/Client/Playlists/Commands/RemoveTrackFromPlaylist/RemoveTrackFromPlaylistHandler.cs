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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public RemoveTrackFromPlaylistHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider, ICacheService cache)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _cache = cache;
    }

    public async Task Handle(RemoveTrackFromPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var isOwner = await _context.Playlists
            .AnyAsync(p => p.Id == request.PlaylistId && p.UserId == userId , cancellationToken);

        if (!isOwner) throw new ForbiddenException("Playlist not found or access denied.");

        var deletedRows = await _context.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId)
            .ExecuteDeleteAsync(cancellationToken);
            
        if (deletedRows > 0)
        {
            await _context.Playlists
                .Where(p => p.Id == request.PlaylistId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.UpdatedAt, _timeProvider.GetUtcNow().UtcDateTime), cancellationToken);

            await _cache.SortedSetRemoveAsync($"playlist:{request.PlaylistId}:tracks", request.TrackId.ToString(), cancellationToken);
        }
    }
}
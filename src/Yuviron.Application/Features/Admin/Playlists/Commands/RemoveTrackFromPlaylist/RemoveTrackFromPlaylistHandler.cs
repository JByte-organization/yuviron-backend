using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.RemoveTrackFromPlaylist;

public sealed class RemoveTrackFromPlaylistHandler : IRequestHandler<RemoveTrackFromPlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public RemoveTrackFromPlaylistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICacheService cache)
    {
        _context = context;
        _timeProvider = timeProvider;
        _cache = cache;
    }

    public async Task<Unit> Handle(RemoveTrackFromPlaylistCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists
                           .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        var deletedRows = await _context.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows > 0)
        {
            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            playlist.NotifyContentChanged(utcNow);
            await _context.SaveChangesAsync(cancellationToken);

            var redisKey = $"playlist:{request.PlaylistId}:tracks";
            await _cache.SortedSetRemoveAsync(redisKey, request.TrackId.ToString(), cancellationToken);
        }

        return Unit.Value;
    }
}
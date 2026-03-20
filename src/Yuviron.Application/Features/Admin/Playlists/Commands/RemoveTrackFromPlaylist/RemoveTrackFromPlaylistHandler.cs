using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.RemoveTrackFromPlaylist;

public sealed class RemoveTrackFromPlaylistHandler : IRequestHandler<RemoveTrackFromPlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public RemoveTrackFromPlaylistHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(RemoveTrackFromPlaylistCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists
                           .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        var playlistTrack = await _context.PlaylistTracks
            .FirstOrDefaultAsync(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId, cancellationToken);

        if (playlistTrack != null)
        {
            int removedPosition = playlistTrack.Position;
            _context.PlaylistTracks.Remove(playlistTrack);
            
            await _context.SaveChangesAsync(cancellationToken);

            await _context.PlaylistTracks
                .Where(pt => pt.PlaylistId == request.PlaylistId && pt.Position > removedPosition)
                .ExecuteUpdateAsync(s => s.SetProperty(pt => pt.Position, pt => pt.Position - 1), cancellationToken);

            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            playlist.NotifyContentChanged(utcNow);
            
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
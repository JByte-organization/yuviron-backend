using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistHandler : IRequestHandler<UpdatePlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdatePlaylistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider, 
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists
                           .Include(p => p.PlaylistTracks)
                           .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.Id);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        playlist.Update(
            request.Title,
            request.Description,
            request.CoverUrl,
            request.IsPublic,
            utcNow
        );

        var tracksToSync = request.Tracks
            .Select(t => (t.TrackId, t.Position));

        playlist.SyncTracks(
            tracksToSync, 
            _currentUser.UserId ?? Guid.Empty, 
            utcNow
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistHandler : IRequestHandler<CreatePlaylistCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public CreatePlaylistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider, 
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var userId = request.IsEditorial ? null : _currentUser.UserId;

        var playlist = Playlist.Create(
            userId,
            request.Title,
            request.Description,
            request.CoverUrl,
            request.IsPublic,
            request.IsEditorial,
            utcNow
        );

        if (request.Tracks != null && request.Tracks.Any())
        {
            var tracksToSync = request.Tracks
                .Select(t => (t.TrackId, t.Position));

            playlist.SyncTracks(tracksToSync, _currentUser.UserId ?? Guid.Empty, utcNow);
        }

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync(cancellationToken);

        return playlist.Id;
    }
}
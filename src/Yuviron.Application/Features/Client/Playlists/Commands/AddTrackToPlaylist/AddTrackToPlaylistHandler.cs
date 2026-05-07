using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Playlists.Commands.AddTrackToPlaylist;

public sealed class AddTrackToPlaylistHandler : IRequestHandler<AddTrackToPlaylistCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public AddTrackToPlaylistHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task Handle(AddTrackToPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId && !p.IsDeleted, cancellationToken);

        if (playlist is null)
            throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        if (playlist.UserId != userId)
            throw new ForbiddenException("Access denied.");

        var trackExists = await _context.Tracks
            .AnyAsync(t => t.Id == request.TrackId && !t.IsDeleted, cancellationToken);
            
        if (!trackExists)
            throw new NotFoundException(nameof(Track), request.TrackId);

        var alreadyInPlaylist = await _context.PlaylistTracks
            .AnyAsync(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId, cancellationToken);

        if (alreadyInPlaylist)
            return; 

        var maxPosition = await _context.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .MaxAsync(pt => (int?)pt.Position, cancellationToken) ?? 0;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var playlistTrack = new PlaylistTrack(
            request.PlaylistId,
            request.TrackId,
            maxPosition + 1,
            userId,
            utcNow
        );

        _context.PlaylistTracks.Add(playlistTrack);

        playlist.NotifyContentChanged(utcNow); 

        await _context.SaveChangesAsync(cancellationToken);
    }
}
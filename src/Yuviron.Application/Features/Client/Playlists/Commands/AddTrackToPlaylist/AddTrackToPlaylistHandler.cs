using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Playlists.Commands.AddTrackToPlaylist;

public sealed class AddTrackToPlaylistHandler : IRequestHandler<AddTrackToPlaylistCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public AddTrackToPlaylistHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider, ICacheService cache)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _cache = cache;
    }

    public async Task Handle(AddTrackToPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId && !p.IsDeleted, cancellationToken);

        if (playlist is null) throw new NotFoundException(nameof(Playlist), request.PlaylistId);
        if (playlist.UserId != userId) throw new ForbiddenException("Access denied.");

        var trackExists = await _context.Tracks.AnyAsync(t => t.Id == request.TrackId && !t.IsDeleted, cancellationToken);
        if (!trackExists) throw new NotFoundException(nameof(Track), request.TrackId);

        if (await _context.PlaylistTracks.AnyAsync(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId, cancellationToken))
            return; 

        var maxPosition = await _context.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .MaxAsync(pt => (double?)pt.Position, cancellationToken) ?? 0.0;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var newPosition = maxPosition + 65536.0;

        var playlistTrack = new PlaylistTrack(request.PlaylistId, request.TrackId, newPosition, userId, utcNow);

        _context.PlaylistTracks.Add(playlistTrack);
        playlist.NotifyContentChanged(utcNow); 

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.SortedSetAddAsync($"playlist:{request.PlaylistId}:tracks", request.TrackId.ToString(), newPosition, cancellationToken);
    }
}
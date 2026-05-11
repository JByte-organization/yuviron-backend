using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.AddTrackToPlaylist;

public sealed class AddTrackToPlaylistHandler : IRequestHandler<AddTrackToPlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public AddTrackToPlaylistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider, 
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Unit> Handle(AddTrackToPlaylistCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId 
            ?? throw new UnauthorizedAccessException("User context is required.");

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        var trackExists = await _context.Tracks.AnyAsync(t => t.Id == request.TrackId, cancellationToken);
        if (!trackExists) throw new NotFoundException(nameof(Track), request.TrackId);

        if (await _context.PlaylistTracks.AnyAsync(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId, cancellationToken))
            return Unit.Value;

        var maxPosition = await _context.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .MaxAsync(pt => (double?)pt.Position, cancellationToken) ?? 0.0;

        var newPosition = maxPosition + 65536.0; 

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var newPlaylistTrack = new PlaylistTrack(request.PlaylistId, request.TrackId, newPosition, currentUserId, utcNow);
        
        _context.PlaylistTracks.Add(newPlaylistTrack);
        playlist.NotifyContentChanged(utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        var redisKey = $"playlist:{request.PlaylistId}:tracks";
        await _cache.SortedSetAddAsync(redisKey, request.TrackId.ToString(), newPosition, cancellationToken);

        return Unit.Value;
    }
}
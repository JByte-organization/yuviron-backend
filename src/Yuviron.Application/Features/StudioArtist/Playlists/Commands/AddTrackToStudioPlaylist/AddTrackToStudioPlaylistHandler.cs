using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.AddTrackToStudioPlaylist;

public sealed class AddTrackToStudioPlaylistHandler : IRequestHandler<AddTrackToStudioPlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public AddTrackToStudioPlaylistHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider, ICacheService cache)
    {
        _context = context; _currentUser = currentUser; _timeProvider = timeProvider; _cache = cache;
    }

    public async Task<Unit> Handle(AddTrackToStudioPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        if (playlist.ArtistId == null) throw new ForbiddenException("This is not a studio artist playlist.");

        var hasPermission = await _context.ArtistTeamMembers
            .HasManagementAccess(playlist.ArtistId.Value, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to manage this playlist.");

        var trackExists = await _context.Tracks.AnyAsync(t => t.Id == request.TrackId && !t.IsDeleted, cancellationToken);
        if (!trackExists) throw new NotFoundException(nameof(Track), request.TrackId);

        var isAlreadyInPlaylist = await _context.PlaylistTracks
            .AnyAsync(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId, cancellationToken);
            
        if (isAlreadyInPlaylist) return Unit.Value;

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

        return Unit.Value;
    }
}
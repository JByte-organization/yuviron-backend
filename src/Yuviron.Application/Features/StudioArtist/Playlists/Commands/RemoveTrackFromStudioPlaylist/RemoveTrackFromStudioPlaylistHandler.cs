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
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.RemoveTrackFromStudioPlaylist;

public sealed class RemoveTrackFromStudioPlaylistHandler : IRequestHandler<RemoveTrackFromStudioPlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public RemoveTrackFromStudioPlaylistHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider, ICacheService cache)
    {
        _context = context; _currentUser = currentUser; _timeProvider = timeProvider; _cache = cache;
    }

    public async Task<Unit> Handle(RemoveTrackFromStudioPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlistInfo = await _context.Playlists
            .AsNoTracking()
            .Where(p => p.Id == request.PlaylistId)
            .Select(p => new { p.Id, p.ArtistId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Playlist", request.PlaylistId);

        if (playlistInfo.ArtistId == null) throw new ForbiddenException("This is not a studio artist playlist.");

        var hasPermission = await _context.ArtistTeamMembers
            .HasManagementAccess(playlistInfo.ArtistId.Value, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to manage this playlist.");

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

        return Unit.Value;
    }
}
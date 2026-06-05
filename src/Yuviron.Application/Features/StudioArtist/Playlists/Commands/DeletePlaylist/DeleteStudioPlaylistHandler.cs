using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.DeletePlaylist;

public sealed class DeleteStudioPlaylistHandler : IRequestHandler<DeleteStudioPlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public DeleteStudioPlaylistHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _context = context; _currentUser = currentUser; _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteStudioPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _context.Playlists
                           .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        if (playlist.ArtistId == null) throw new ForbiddenException("This is not a studio artist playlist.");

        var hasPermission = await _context.ArtistTeamMembers
            .HasManagementAccess(playlist.ArtistId.Value, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to delete this playlist.");

        // Soft Delete (как заложено в доменной модели)
        playlist.Delete(_timeProvider.GetUtcNow().UtcDateTime);

        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
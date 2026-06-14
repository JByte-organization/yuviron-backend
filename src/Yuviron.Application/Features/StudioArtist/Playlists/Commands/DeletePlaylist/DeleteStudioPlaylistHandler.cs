using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public DeleteStudioPlaylistHandler(ICatalogContext catalogContext, ILibraryContext libraryContext, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext; _currentUser = currentUser; _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteStudioPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _libraryContext.Playlists
                           .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        if (playlist.ArtistId == null) throw new ForbiddenException("This is not a studio artist playlist.");

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasManagementAccess(playlist.ArtistId.Value, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to delete this playlist.");

        // Soft Delete (как заложено в доменной модели)
        playlist.Delete(_timeProvider.GetUtcNow().UtcDateTime);

        await _catalogContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
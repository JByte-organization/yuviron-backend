using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Albums.Commands.DeleteAlbum;

public sealed class DeleteAlbumHandler : IRequestHandler<DeleteAlbumCommand>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public DeleteAlbumHandler(
        ICatalogContext catalogContext,
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteAlbumCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var album = await _catalogContext.Albums
            .Include(a => a.AlbumArtists)
            .FirstOrDefaultAsync(a => a.Id == request.AlbumId, cancellationToken);

        if (album == null)
        {
            throw new NotFoundException(nameof(Album), request.AlbumId);
        }

        var albumArtistIds = album.AlbumArtists.Select(aa => aa.ArtistId).ToList();
        
        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasManagementAccess(albumArtistIds, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission)
        {
            throw new ForbiddenException("You do not have permission to delete this album.");
        }

        album.Delete(utcNow);

        await _catalogContext.SaveChangesAsync(cancellationToken);
    }
}
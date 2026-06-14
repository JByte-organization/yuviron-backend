using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Playlists.Commands.DeletePlaylist;

public sealed class DeletePlaylistHandler : IRequestHandler<DeletePlaylistCommand>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public DeletePlaylistHandler(ILibraryContext libraryContext, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _libraryContext = libraryContext;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeletePlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _libraryContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId , cancellationToken);

        if (playlist is null)
            throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        if (playlist.UserId != userId)
            throw new ForbiddenException("You can only delete your own playlists.");

        playlist.Delete(_timeProvider.GetUtcNow().UtcDateTime);

        await _libraryContext.SaveChangesAsync(cancellationToken);
    }
}
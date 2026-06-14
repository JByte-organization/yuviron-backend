using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Library.Commands.RemovePlaylistFromFavorites;

public sealed class RemovePlaylistFromFavoritesHandler : IRequestHandler<RemovePlaylistFromFavoritesCommand, Unit>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public RemovePlaylistFromFavoritesHandler(ILibraryContext libraryContext, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _libraryContext = libraryContext; _currentUserService = currentUserService; _cacheService = cacheService;
    }

    public async Task<Unit> Handle(RemovePlaylistFromFavoritesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var entity = await _libraryContext.UserSavedPlaylists
            .FirstOrDefaultAsync(usp => usp.UserId == userId && usp.PlaylistId == request.PlaylistId, cancellationToken);

        if (entity != null)
        {
            _libraryContext.Remove(entity);
            await _libraryContext.SaveChangesAsync(cancellationToken);
            
            await _cacheService.SetRemoveAsync($"user:{userId}:saved_playlists", request.PlaylistId.ToString(), cancellationToken);
        }

        return Unit.Value;
    }
}
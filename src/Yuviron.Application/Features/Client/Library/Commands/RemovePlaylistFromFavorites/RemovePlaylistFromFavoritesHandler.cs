using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Library.Commands.RemovePlaylistFromFavorites;

public sealed class RemovePlaylistFromFavoritesHandler : IRequestHandler<RemovePlaylistFromFavoritesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public RemovePlaylistFromFavoritesHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context; _currentUserService = currentUserService; _cacheService = cacheService;
    }

    public async Task<Unit> Handle(RemovePlaylistFromFavoritesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var entity = await _context.UserSavedPlaylists
            .FirstOrDefaultAsync(usp => usp.UserId == userId && usp.PlaylistId == request.PlaylistId, cancellationToken);

        if (entity != null)
        {
            _context.UserSavedPlaylists.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            await _cacheService.SetRemoveAsync($"user:{userId}:saved_playlists", request.PlaylistId.ToString(), cancellationToken);
        }

        return Unit.Value;
    }
}
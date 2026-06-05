using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Library.Commands.RemoveAlbumFromFavorites;

public sealed class RemoveAlbumFromFavoritesHandler : IRequestHandler<RemoveAlbumFromFavoritesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public RemoveAlbumFromFavoritesHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context; _currentUserService = currentUserService; _cacheService = cacheService;
    }

    public async Task<Unit> Handle(RemoveAlbumFromFavoritesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var entity = await _context.UserSavedAlbums.FirstOrDefaultAsync(usa => usa.UserId == userId && usa.AlbumId == request.AlbumId, cancellationToken);

        if (entity != null)
        {
            _context.UserSavedAlbums.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            await _cacheService.SetRemoveAsync($"user:{userId}:saved_albums", request.AlbumId.ToString(), cancellationToken);
        }

        return Unit.Value;
    }
}
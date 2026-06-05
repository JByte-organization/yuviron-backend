using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Library.Commands.AddAlbumToFavorites;

public sealed class AddAlbumToFavoritesHandler : IRequestHandler<AddAlbumToFavoritesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public AddAlbumToFavoritesHandler(IApplicationDbContext context, TimeProvider timeProvider, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context; _timeProvider = timeProvider; _currentUserService = currentUserService; _cacheService = cacheService;
    }

    public async Task<Unit> Handle(AddAlbumToFavoritesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var albumExists = await _context.Albums.AvailableForPublic(utcNow).AnyAsync(a => a.Id == request.AlbumId, cancellationToken);
        if (!albumExists) throw new NotFoundException(nameof(Album), request.AlbumId);

        var alreadySaved = await _context.UserSavedAlbums.AnyAsync(usa => usa.UserId == userId && usa.AlbumId == request.AlbumId, cancellationToken);

        if (!alreadySaved)
        {
            _context.UserSavedAlbums.Add(new UserSavedAlbum(userId, request.AlbumId, utcNow));
            await _context.SaveChangesAsync(cancellationToken);
            
            await _cacheService.SetAddAsync($"user:{userId}:saved_albums", request.AlbumId.ToString(), cancellationToken);
        }

        return Unit.Value;
    }
}
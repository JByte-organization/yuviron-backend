using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public AddAlbumToFavoritesHandler(ICatalogContext catalogContext, ILibraryContext libraryContext, TimeProvider timeProvider, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext; _timeProvider = timeProvider; _currentUserService = currentUserService; _cacheService = cacheService;
    }

    public async Task<Unit> Handle(AddAlbumToFavoritesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var albumExists = await _catalogContext.Albums.AvailableForPublic(utcNow).AnyAsync(a => a.Id == request.AlbumId, cancellationToken);
        if (!albumExists) throw new NotFoundException(nameof(Album), request.AlbumId);

        var alreadySaved = await _libraryContext.UserSavedAlbums.AnyAsync(usa => usa.UserId == userId && usa.AlbumId == request.AlbumId, cancellationToken);

        if (!alreadySaved)
        {
            _libraryContext.Add(new UserSavedAlbum(userId, request.AlbumId, utcNow));
            await _catalogContext.SaveChangesAsync(cancellationToken);
            
            await _cacheService.SetAddAsync($"user:{userId}:saved_albums", request.AlbumId.ToString(), cancellationToken);
        }

        return Unit.Value;
    }
}
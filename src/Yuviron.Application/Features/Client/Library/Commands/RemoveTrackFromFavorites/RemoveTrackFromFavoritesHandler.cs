using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Library.Commands.RemoveTrackFromFavorites;

public sealed class RemoveTrackFromFavoritesHandler : IRequestHandler<RemoveTrackFromFavoritesCommand, Unit>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService; 

    public RemoveTrackFromFavoritesHandler(ILibraryContext libraryContext, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _libraryContext = libraryContext; _currentUserService = currentUserService; _cacheService = cacheService;
    }

    public async Task<Unit> Handle(RemoveTrackFromFavoritesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var entity = await _libraryContext.UserSavedTracks
            .FirstOrDefaultAsync(ust => ust.UserId == userId && ust.TrackId == request.TrackId, cancellationToken);

        if (entity != null)
        {
            _libraryContext.Remove(entity);
            await _libraryContext.SaveChangesAsync(cancellationToken);
            
            await _cacheService.SetRemoveAsync($"user:{userId}:saved_tracks", request.TrackId.ToString(), cancellationToken);
        }

        return Unit.Value;
    }
}
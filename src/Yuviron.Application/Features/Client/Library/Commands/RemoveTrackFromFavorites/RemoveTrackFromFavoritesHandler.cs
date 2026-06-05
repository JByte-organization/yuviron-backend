using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Library.Commands.RemoveTrackFromFavorites;

public sealed class RemoveTrackFromFavoritesHandler : IRequestHandler<RemoveTrackFromFavoritesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService; 

    public RemoveTrackFromFavoritesHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _context = context; _currentUserService = currentUserService; _cacheService = cacheService;
    }

    public async Task<Unit> Handle(RemoveTrackFromFavoritesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var entity = await _context.UserSavedTracks
            .FirstOrDefaultAsync(ust => ust.UserId == userId && ust.TrackId == request.TrackId, cancellationToken);

        if (entity != null)
        {
            _context.UserSavedTracks.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            await _cacheService.SetRemoveAsync($"user:{userId}:saved_tracks", request.TrackId.ToString(), cancellationToken);
        }

        return Unit.Value;
    }
}
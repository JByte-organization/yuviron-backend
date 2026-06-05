using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching; // <-- Подключаем кэш
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Library.Commands.AddTrackToFavorites;

public sealed class AddTrackToFavoritesHandler : IRequestHandler<AddTrackToFavoritesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService; // <-- Добавили

    public AddTrackToFavoritesHandler(
        IApplicationDbContext context,
        TimeProvider timeProvider,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<Unit> Handle(AddTrackToFavoritesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var trackExists = await _context.Tracks
            .AvailableForPublic(utcNow) 
            .AnyAsync(t => t.Id == request.TrackId, cancellationToken);

        if (!trackExists) throw new NotFoundException(nameof(Track), request.TrackId);

        var alreadySaved = await _context.UserSavedTracks
            .AnyAsync(ust => ust.UserId == userId && ust.TrackId == request.TrackId, cancellationToken);

        if (!alreadySaved)
        {
            _context.UserSavedTracks.Add(new UserSavedTrack(userId, request.TrackId, utcNow));
            await _context.SaveChangesAsync(cancellationToken);
            
            await _cacheService.SetAddAsync($"user:{userId}:saved_tracks", request.TrackId.ToString(), cancellationToken);
        }

        return Unit.Value;
    }
}
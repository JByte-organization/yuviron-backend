using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Library.Commands.AddPlaylistToFavorites;

public sealed class AddPlaylistToFavoritesHandler : IRequestHandler<AddPlaylistToFavoritesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IEventBus _eventBus;

    public AddPlaylistToFavoritesHandler(IApplicationDbContext context, TimeProvider timeProvider, ICurrentUserService currentUserService, ICacheService cacheService, IEventBus eventBus)
    {
        _context = context; _timeProvider = timeProvider; _currentUserService = currentUserService; _cacheService = cacheService; _eventBus = eventBus;
    }

    public async Task<Unit> Handle(AddPlaylistToFavoritesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId && !p.IsDeleted && 
                          (p.Visibility == PlaylistVisibility.Public || p.UserId == userId), cancellationToken);

        if (playlist == null) throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        var alreadySaved = await _context.UserSavedPlaylists
            .AnyAsync(usp => usp.UserId == userId && usp.PlaylistId == request.PlaylistId, cancellationToken);

        if (!alreadySaved)
        {
            _context.UserSavedPlaylists.Add(new UserSavedPlaylist(userId, request.PlaylistId, utcNow));
            await _context.SaveChangesAsync(cancellationToken);
            
            await _cacheService.SetAddAsync($"user:{userId}:saved_playlists", request.PlaylistId.ToString(), cancellationToken);

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId, cancellationToken);
            var userName = userProfile?.FirstName ?? "Користувач";

            if (playlist.UserId.HasValue)
            {
                await _eventBus.PublishAsync(new PlaylistAddedToFavoritesEvent(playlist.Id, playlist.Title, playlist.UserId.Value, userId, userName), cancellationToken);
            }
        }

        return Unit.Value;
    }
}

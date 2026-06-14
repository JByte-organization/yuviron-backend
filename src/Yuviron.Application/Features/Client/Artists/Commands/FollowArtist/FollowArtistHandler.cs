using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Commands.FollowArtist;

public sealed class FollowArtistHandler : IRequestHandler<FollowArtistCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEventBus _eventBus;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cacheService;

    public FollowArtistHandler(
        ICatalogContext catalogContext, ILibraryContext libraryContext,
        ICurrentUserService currentUserService,
        IEventBus eventBus,
        TimeProvider timeProvider,
        ICacheService cacheService)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _currentUserService = currentUserService;
        _eventBus = eventBus;
        _timeProvider = timeProvider;
        _cacheService = cacheService;
    }

    public async Task<Unit> Handle(FollowArtistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var artist = await _catalogContext.Artists
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var alreadyFollowing = await _libraryContext.UserFollowArtists
            .AnyAsync(f => f.UserId == userId && f.ArtistId == request.ArtistId, cancellationToken);

        if (alreadyFollowing) return Unit.Value;

        var previousFollowersCount = await _libraryContext.UserFollowArtists
            .CountAsync(f => f.ArtistId == request.ArtistId, cancellationToken);

        _libraryContext.Add(new UserFollowArtist(
            userId,
            request.ArtistId,
            true,
            _timeProvider.GetUtcNow().UtcDateTime
        ));

        await _catalogContext.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new UserFollowedArtistEvent(userId, request.ArtistId), cancellationToken);

        var followersCount = await _libraryContext.UserFollowArtists.CountAsync(f => f.ArtistId == request.ArtistId, cancellationToken);
        
        if (previousFollowersCount < 1000 && followersCount >= 1000)
        {
            var cacheKey = $"milestone:artist:{request.ArtistId}:followers:1000";
            var alreadyNotified = await _cacheService.GetAsync<bool>(cacheKey, cancellationToken);
            
            if (!alreadyNotified)
            {
                await _eventBus.PublishAsync(
                    new ArtistFollowersMilestoneReachedEvent(request.ArtistId, artist.Name, followersCount, 1000),
                    cancellationToken);
                
                await _cacheService.SetAsync(cacheKey, true, TimeSpan.FromDays(3650), cancellationToken);
            }
        }

        return Unit.Value;
    }
}
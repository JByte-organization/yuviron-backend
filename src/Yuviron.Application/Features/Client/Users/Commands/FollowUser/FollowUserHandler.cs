using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Users.Commands.FollowUser;

public sealed class FollowUserHandler : IRequestHandler<FollowUserCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly IProfileContext _profileContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public FollowUserHandler(IIdentityContext identityContext, IProfileContext profileContext, ILibraryContext libraryContext, ICurrentUserService currentUserService, TimeProvider timeProvider, IEventBus eventBus)
    {
        _identityContext = identityContext;
        _profileContext = profileContext;
        _libraryContext = libraryContext;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        if (currentUserId == request.TargetUserId)
            throw new InvalidOperationException("You cannot follow yourself.");

        var targetUserExists = await _identityContext.Users.AnyAsync(u => u.Id == request.TargetUserId , cancellationToken);
        if (!targetUserExists) throw new NotFoundException(nameof(User), request.TargetUserId);

        var alreadyFollowing = await _libraryContext.UserFollowUsers
            .AnyAsync(f => f.FollowerId == currentUserId && f.FolloweeId == request.TargetUserId, cancellationToken);

        if (alreadyFollowing) return Unit.Value; 

        _libraryContext.Add(new UserFollowUser(currentUserId, request.TargetUserId, _timeProvider.GetUtcNow().UtcDateTime));
        await _identityContext.SaveChangesAsync(cancellationToken);

        var follower = await _profileContext.UserProfiles.FirstOrDefaultAsync(p => p.Id == currentUserId, cancellationToken);
        var followerName = follower?.FirstName ?? "Користувач";

        await _eventBus.PublishAsync(new UserFollowedUserEvent(currentUserId, request.TargetUserId, followerName), cancellationToken);

        return Unit.Value;
    }
}

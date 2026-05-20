using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Users.Commands.FollowUser;

public sealed class FollowUserHandler : IRequestHandler<FollowUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public FollowUserHandler(IApplicationDbContext context, ICurrentUserService currentUserService, TimeProvider timeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        if (currentUserId == request.TargetUserId)
            throw new InvalidOperationException("You cannot follow yourself.");

        var targetUserExists = await _context.Users.AnyAsync(u => u.Id == request.TargetUserId && !u.IsDeleted, cancellationToken);
        if (!targetUserExists) throw new NotFoundException(nameof(User), request.TargetUserId);

        var alreadyFollowing = await _context.UserFollowUsers
            .AnyAsync(f => f.FollowerId == currentUserId && f.FolloweeId == request.TargetUserId, cancellationToken);

        if (alreadyFollowing) return Unit.Value; 

        _context.UserFollowUsers.Add(new UserFollowUser(currentUserId, request.TargetUserId, _timeProvider.GetUtcNow().UtcDateTime));
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
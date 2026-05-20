using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Users.Commands.UnfollowUser;

public sealed class UnfollowUserHandler : IRequestHandler<UnfollowUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UnfollowUserHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var followRecord = await _context.UserFollowUsers
            .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FolloweeId == request.TargetUserId, cancellationToken);

        if (followRecord != null)
        {
            _context.UserFollowUsers.Remove(followRecord);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
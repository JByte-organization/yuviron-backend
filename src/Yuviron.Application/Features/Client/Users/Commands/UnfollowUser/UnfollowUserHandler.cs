using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Users.Commands.UnfollowUser;

public sealed class UnfollowUserHandler : IRequestHandler<UnfollowUserCommand, Unit>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUserService;

    public UnfollowUserHandler(ILibraryContext libraryContext, ICurrentUserService currentUserService)
    {
        _libraryContext = libraryContext;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var followRecord = await _libraryContext.UserFollowUsers
            .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FolloweeId == request.TargetUserId, cancellationToken);

        if (followRecord != null)
        {
            _libraryContext.Remove(followRecord);
            await _libraryContext.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Users.Commands.UpdateProfile;

public sealed class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public UpdateProfileHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var profile = await _context.UserProfiles
                          .FirstOrDefaultAsync(p => p.Id == userId, cancellationToken)
                      ?? throw new NotFoundException(nameof(UserProfile), userId);

        profile.UpdateDetails(
            request.DisplayName.Trim(),
            profile.AvatarUrl, 
            request.Country,
            request.Bio,
            request.DateOfBirth,
            request.Gender,
            _timeProvider.GetUtcNow().UtcDateTime
        );

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
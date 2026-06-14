using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Users.Commands.UpdateAccountDetails;

public sealed class UpdateAccountDetailsHandler : IRequestHandler<UpdateAccountDetailsCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICurrentUserService _currentUser;

    public UpdateAccountDetailsHandler(IIdentityContext identityContext, ICurrentUserService currentUser)
    {
        _identityContext = identityContext;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateAccountDetailsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = DateTime.UtcNow;
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var emailTaken = await _identityContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id != userId && u.Email == normalizedEmail, cancellationToken);

        if (emailTaken)
        {
            throw new UserAlreadyExistsException(normalizedEmail);
        }

        var user = await _identityContext.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user?.Profile is null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        user.UpdateAccountDetails(normalizedEmail, request.AcceptMarketing, utcNow);
        user.Profile.UpdateAccountDetails(request.Country, request.DateOfBirth, request.Gender, utcNow);

        await _identityContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

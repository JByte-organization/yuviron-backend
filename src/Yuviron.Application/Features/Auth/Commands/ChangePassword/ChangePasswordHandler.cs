using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;

    public ChangePasswordHandler(
        IIdentityContext identityContext, 
        ICurrentUserService currentUser, 
        IPasswordHasher passwordHasher, 
        TimeProvider timeProvider)
    {
        _identityContext = identityContext;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var user = await _identityContext.Users
                       .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), userId);

        if (!_passwordHasher.Verify(request.OldPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("The old password is incorrect.");
        }

        var newHash = _passwordHasher.Hash(request.NewPassword);
        
        user.SetPasswordHash(newHash, _timeProvider.GetUtcNow().UtcDateTime);

        await _identityContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
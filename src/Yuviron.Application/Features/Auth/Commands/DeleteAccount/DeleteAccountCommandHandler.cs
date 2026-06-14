using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Auth.Commands.DeleteAccount;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICurrentUserService _currentUser;

    public DeleteAccountCommandHandler(IIdentityContext identityContext, ICurrentUserService currentUser)
    {
        _identityContext = identityContext;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;

        var user = await _identityContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        user.Delete(DateTime.UtcNow);

        // Also revoke tokens immediately
        foreach (var token in user.RefreshTokens)
        {
            token.Revoke(DateTime.UtcNow);
        }

        await _identityContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}


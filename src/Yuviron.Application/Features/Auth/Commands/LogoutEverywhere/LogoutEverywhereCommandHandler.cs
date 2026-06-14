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

namespace Yuviron.Application.Features.Auth.Commands.LogoutEverywhere;

public class LogoutEverywhereCommandHandler : IRequestHandler<LogoutEverywhereCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICurrentUserService _currentUser;

    public LogoutEverywhereCommandHandler(IIdentityContext identityContext, ICurrentUserService currentUser)
    {
        _identityContext = identityContext;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(LogoutEverywhereCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;

        var user = await _identityContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        var utcNow = DateTime.UtcNow;
        foreach (var token in user.RefreshTokens)
        {
            token.Revoke(utcNow);
        }

        var devices = await _identityContext.UserDevices
            .Where(d => d.UserId == userId)
            .ToListAsync(cancellationToken);
            
        _identityContext.RemoveRange(devices);

        await _identityContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}


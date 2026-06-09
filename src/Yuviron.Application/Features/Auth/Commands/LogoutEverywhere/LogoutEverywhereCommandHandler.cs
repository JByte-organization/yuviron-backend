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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public LogoutEverywhereCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(LogoutEverywhereCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;

        var user = await _context.Users
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

        var devices = await _context.UserDevices
            .Where(d => d.UserId == userId)
            .ToListAsync(cancellationToken);
            
        _context.UserDevices.RemoveRange(devices);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}


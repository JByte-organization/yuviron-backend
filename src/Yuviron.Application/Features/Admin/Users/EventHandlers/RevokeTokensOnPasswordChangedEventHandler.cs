using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Users.EventHandlers;

public sealed class RevokeTokensOnPasswordChangedEventHandler : INotificationHandler<UserPasswordChangedEvent>
{
    private readonly IApplicationDbContext _context;

    public RevokeTokensOnPasswordChangedEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UserPasswordChangedEvent notification, CancellationToken cancellationToken)
    {
        var tokensToRemove = await _context.RefreshTokens
            .Where(rt => rt.UserId == notification.UserId)
            .ToListAsync(cancellationToken);

        if (!tokensToRemove.Any())
        {
            return; 
        }

        _context.RefreshTokens.RemoveRange(tokensToRemove);
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}
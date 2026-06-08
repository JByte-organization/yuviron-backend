using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Users.Commands.UnblockUser;

public sealed class UnblockUserHandler : IRequestHandler<UnblockUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public UnblockUserHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IEventBus eventBus)
    {
        _context = context;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
                       .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), request.UserId);

        if (user.AccountState == AccountState.Deleted)
        {
            throw new InvalidOperationException("Cannot unblock a deleted user.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var wasBanned = user.AccountState == AccountState.Banned;

        var activeBlocks = await _context.UserBlocks
            .Where(b => b.UserId == request.UserId && b.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var block in activeBlocks)
        {
            block.Deactivate(utcNow);
        }

        if (user.AccountState == AccountState.Banned)
        {
            user.SetAccountState(AccountState.Active, utcNow);
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (activeBlocks.Any() || wasBanned)
        {
            await _eventBus.PublishAsync(new UserUnblockedEvent(user.Id), cancellationToken);
        }

        return Unit.Value;
    }
}

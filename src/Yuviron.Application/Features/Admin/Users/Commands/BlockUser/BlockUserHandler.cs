using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Users.Commands.BlockUser;

public sealed class BlockUserHandler : IRequestHandler<BlockUserCommand, Guid>
{
    private readonly IIdentityContext _identityContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEventBus _eventBus;

    public BlockUserHandler(
        IIdentityContext identityContext, 
        TimeProvider timeProvider,
        ICurrentUserService currentUserService,
        IEventBus eventBus)
    {
        _identityContext = identityContext;
        _timeProvider = timeProvider;
        _currentUserService = currentUserService;
        _eventBus = eventBus;
    }

    public async Task<Guid> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityContext.Users
                       .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), request.UserId);

        if (user.AccountState == AccountState.Deleted)
        {
            throw new InvalidOperationException("Cannot block a deleted user.");
        }

        var adminId = _currentUserService.UserId 
                      ?? throw new UnauthorizedAccessException("Admin identityContext is required for this operation."); 

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var block = UserBlock.Create(
            user.Id,
            request.BlockType,
            request.ReasonCode.Trim(),
            request.Description.Trim(),
            adminId, 
            utcNow,
            request.EndsAt,
            utcNow);

        _identityContext.Add(block);

        if (request.BlockType == BlockType.Full || request.BlockType == BlockType.Login)
        {
            user.SetAccountState(AccountState.Banned, utcNow);
        }

        await _identityContext.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new UserBlockedEvent(user.Id, request.ReasonCode, request.Description, request.EndsAt),
            cancellationToken);

        return block.Id;
    }
}

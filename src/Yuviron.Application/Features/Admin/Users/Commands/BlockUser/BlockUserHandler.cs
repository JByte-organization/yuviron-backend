using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Users.Commands.BlockUser;

public sealed class BlockUserHandler : IRequestHandler<BlockUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUserService;

    public BlockUserHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
                       .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), request.UserId);

        if (user.AccountState == AccountState.Deleted)
        {
            throw new InvalidOperationException("Cannot block a deleted user.");
        }

        var adminId = _currentUserService.UserId 
                      ?? throw new UnauthorizedAccessException("Admin context is required for this operation."); 

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

        _context.UserBlocks.Add(block);

        if (request.BlockType == BlockType.Full || request.BlockType == BlockType.Login)
        {
            user.SetAccountState(AccountState.Banned, utcNow);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return block.Id;
    }
}
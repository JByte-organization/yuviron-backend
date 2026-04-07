using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Users.Commands.DeleteUser;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public DeleteUserHandler(
        IApplicationDbContext context,
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
                       .Include(u => u.Profile)
                       .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), request.UserId);

        var avatarUrlToDelete = user.Profile!.AvatarUrl;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        user.Delete(utcNow);
        
        user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id));

        if (!string.IsNullOrWhiteSpace(avatarUrlToDelete))
        {
            user.AddDomainEvent(new FileNeedsDeletionEvent(avatarUrlToDelete));
        }
        
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Users.Commands.DeleteUser;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly TimeProvider _timeProvider;

    public DeleteUserHandler(IIdentityContext identityContext, TimeProvider timeProvider)
    {
        _identityContext = identityContext;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityContext.Users
                       .Include(u => u.Profile)
                       .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), request.UserId);

        user.Delete(_timeProvider.GetUtcNow().UtcDateTime);
        
        user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id));

        await _identityContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
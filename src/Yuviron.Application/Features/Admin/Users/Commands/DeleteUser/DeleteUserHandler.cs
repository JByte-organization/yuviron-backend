using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IPermissionService _permissionService;
    private readonly TimeProvider _timeProvider;

    public DeleteUserCommandHandler(
        IApplicationDbContext context, 
        IPermissionService permissionService,
        TimeProvider timeProvider)
    {
        _context = context;
        _permissionService = permissionService;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
                       .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), request.UserId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // Мягкое удаление вместо Remove!
        user.Delete(utcNow);
        
        await _context.SaveChangesAsync(cancellationToken);

        // Инвалидируем кэш прав (очень правильный шаг, молодец, что добавил!)
        await _permissionService.InvalidatePermissionsAsync(request.UserId, cancellationToken);

        return Unit.Value;
    }
}
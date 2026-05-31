using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Notifications.Commands.MarkAsRead;

public sealed class MarkNotificationAsReadHandler : IRequestHandler<MarkNotificationAsReadCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MarkNotificationAsReadHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == userId, cancellationToken);

        if (notification == null)
            throw new NotFoundException(nameof(Domain.Entities.Notification), request.NotificationId);

        notification.MarkAsRead();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Notifications.Commands.MarkAllAsRead;

public sealed class MarkAllNotificationsAsReadHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Unit>
{
    private readonly ISystemContext _systemContext;
    private readonly ICurrentUserService _currentUserService;

    public MarkAllNotificationsAsReadHandler(ISystemContext systemContext, ICurrentUserService currentUserService)
    {
        _systemContext = systemContext;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        await _systemContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);

        return Unit.Value;
    }
}
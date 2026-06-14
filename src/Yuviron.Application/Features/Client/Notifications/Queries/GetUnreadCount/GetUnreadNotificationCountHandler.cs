using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Notifications.Queries.GetUnreadCount;

public sealed class GetUnreadNotificationCountHandler : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly ISystemContext _systemContext;
    private readonly ICurrentUserService _currentUserService;

    public GetUnreadNotificationCountHandler(ISystemContext systemContext, ICurrentUserService currentUserService)
    {
        _systemContext = systemContext;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        return await _systemContext.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }
}
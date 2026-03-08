using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Users.Events;

public sealed class UserPermissionsChangedEventHandler : INotificationHandler<UserPermissionsChangedEvent>
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<UserPermissionsChangedEventHandler> _logger;

    public UserPermissionsChangedEventHandler(
        IPermissionService permissionService,
        ILogger<UserPermissionsChangedEventHandler> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task Handle(UserPermissionsChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Outbox triggered cache invalidation for User: {UserId}", notification.UserId);
        
        await _permissionService.InvalidatePermissionsAsync(notification.UserId, cancellationToken);
    }
}
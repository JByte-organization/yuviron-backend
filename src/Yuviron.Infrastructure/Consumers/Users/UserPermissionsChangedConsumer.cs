using MassTransit;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class UserPermissionsChangedConsumer : IConsumer<UserPermissionsChangedEvent>
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<UserPermissionsChangedConsumer> _logger;

    public UserPermissionsChangedConsumer(
        IPermissionService permissionService,
        ILogger<UserPermissionsChangedConsumer> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserPermissionsChangedEvent> context)
    {
        _logger.LogInformation("MassTransit triggered cache invalidation for User: {UserId}", context.Message.UserId);
        
        await _permissionService.InvalidatePermissionsAsync(context.Message.UserId, context.CancellationToken);
    }
}
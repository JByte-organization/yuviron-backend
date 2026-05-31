using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common.Models;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.SignalR;

namespace Yuviron.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IHubContext<AppHub, IYuvironClient> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext context, 
        TimeProvider timeProvider, 
        IHubContext<AppHub, IYuvironClient> hubContext, 
        ILogger<NotificationService> logger)
    {
        _context = context;
        _timeProvider = timeProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToUserAsync(Guid userId, string title, string body, NotificationEntityType? entityType = null, Guid? entityId = null, CancellationToken cancellationToken = default)
    {
        await SendToUsersAsync(new[] { userId }, title, body, entityType, entityId, cancellationToken);
    }

    public async Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body, NotificationEntityType? entityType = null, Guid? entityId = null, CancellationToken cancellationToken = default)
    {
        var usersList = userIds.Distinct().ToList();
        if (!usersList.Any()) return;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var notifications = usersList.Select(userId => 
            Notification.Create(userId, title, body, entityType, entityId, utcNow)
        ).ToList();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(cancellationToken);

        var sampleNotif = notifications.First(); 
        var dto = new NotificationDto(
            sampleNotif.Id, title, body, entityType?.ToString(), entityId, false, utcNow);

        var connectionIds = usersList.Select(id => id.ToString()).ToList();
        await _hubContext.Clients.Users(connectionIds).ReceiveNotification(dto);

        _logger.LogInformation("Sent notification '{Title}' to {Count} users.", title, usersList.Count);
    }
}
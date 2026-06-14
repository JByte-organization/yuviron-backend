using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Features.Client.Notifications.Preferences;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.SignalR;

namespace Yuviron.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _profileContext;
    private readonly AppDbContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly IHubContext<AppHub, IYuvironClient> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        AppDbContext profileContext, AppDbContext systemContext,
        TimeProvider timeProvider,
        IHubContext<AppHub, IYuvironClient> hubContext,
        ILogger<NotificationService> logger)
    {
        _profileContext = profileContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToUserAsync(
        Guid userId,
        NotificationCategory category,
        string type,
        string title,
        string body,
        NotificationEntityType? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        await SendToUsersAsync(new[] { userId }, category, type, title, body, entityType, entityId, cancellationToken);
    }

    public async Task SendToUsersAsync(
        IEnumerable<Guid> userIds,
        NotificationCategory category,
        string type,
        string title,
        string body,
        NotificationEntityType? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        var usersList = userIds.Distinct().ToList();
        if (!usersList.Any()) return;

        var preferences = await _profileContext.UserNotificationPreferences
            .AsNoTracking()
            .Where(x => usersList.Contains(x.UserId))
            .ToListAsync(cancellationToken);

        var targetUserIds = usersList
            .Where(userId => Allows(userId, category, type, preferences))
            .ToList();

        if (!targetUserIds.Any())
        {
            _logger.LogInformation(
                "Skipped notification '{Type}' for {Count} users because notification preference is disabled.",
                type,
                usersList.Count);
            return;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var notifications = targetUserIds.Select(userId =>
            Notification.Create(userId, category, type, title, body, entityType, entityId, utcNow)
        ).ToList();

        _systemContext.AddRange(notifications);
        await _profileContext.SaveChangesAsync(cancellationToken);

        var sampleNotif = notifications.First();
        var dto = new NotificationDto(
            sampleNotif.Id,
            category.ToString(),
            type,
            title,
            body,
            entityType?.ToString(),
            entityId,
            false,
            utcNow);

        var connectionIds = targetUserIds.Select(id => id.ToString()).ToList();
        await _hubContext.Clients.Users(connectionIds).ReceiveNotification(dto);

        _logger.LogInformation("Sent notification '{Type}' to {Count} users.", type, targetUserIds.Count);
    }

    private static bool Allows(
        Guid userId,
        NotificationCategory category,
        string type,
        IReadOnlyList<UserNotificationPreference> preferences)
    {
        var exact = preferences.FirstOrDefault(x =>
            x.UserId == userId &&
            x.Category == category &&
            x.Code.Equals(type, StringComparison.OrdinalIgnoreCase));

        if (exact != null)
        {
            return exact.Enabled;
        }

        var categoryDefault = preferences.FirstOrDefault(x =>
            x.UserId == userId &&
            x.Category == category &&
            x.Code.Equals(NotificationPreferenceCatalog.CategoryAllCode, StringComparison.OrdinalIgnoreCase));

        return categoryDefault?.Enabled ?? true;
    }
}

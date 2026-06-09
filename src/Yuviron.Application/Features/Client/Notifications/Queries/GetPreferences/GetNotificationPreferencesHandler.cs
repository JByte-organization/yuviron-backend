using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Notifications.Preferences;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Notifications.Queries.GetPreferences;

public sealed class GetNotificationPreferencesHandler : IRequestHandler<GetNotificationPreferencesQuery, NotificationPreferencesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationPreferencesHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<NotificationPreferencesDto> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var preferences = await _context.UserNotificationPreferences
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var groups = NotificationPreferenceCatalog.GetGroups()
            .Select(group => new NotificationPreferenceGroupDto(
                group.Category,
                group.Title,
                group.Items.Select(item => new NotificationPreferenceItemDto(
                    item.Code,
                    item.Title,
                    ResolveEnabled(group.Category, item.Code, preferences),
                    item.DefaultEnabled,
                    item.IsCategoryDefault)).ToList()))
            .ToList();

        return new NotificationPreferencesDto(groups);
    }

    private static bool ResolveEnabled(
        Domain.Enums.NotificationCategory category,
        string code,
        IReadOnlyList<UserNotificationPreference> preferences)
    {
        var exact = preferences.FirstOrDefault(x =>
            x.Category == category &&
            x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

        if (exact != null)
        {
            return exact.Enabled;
        }

        var categoryDefault = preferences.FirstOrDefault(x =>
            x.Category == category &&
            x.Code.Equals(NotificationPreferenceCatalog.CategoryAllCode, StringComparison.OrdinalIgnoreCase));

        return categoryDefault?.Enabled ?? true;
    }
}

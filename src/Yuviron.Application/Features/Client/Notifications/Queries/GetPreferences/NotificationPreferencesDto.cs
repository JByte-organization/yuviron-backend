using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Notifications.Queries.GetPreferences;

public sealed record NotificationPreferencesDto(
    IReadOnlyList<NotificationPreferenceGroupDto> Groups);

public sealed record NotificationPreferenceGroupDto(
    NotificationCategory Category,
    string Title,
    IReadOnlyList<NotificationPreferenceItemDto> Items);

public sealed record NotificationPreferenceItemDto(
    string Code,
    string Title,
    bool Enabled,
    bool DefaultEnabled,
    bool IsCategoryDefault);

using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Notifications.Commands.UpdatePreferences;

public sealed record UpdateNotificationPreferenceCommand(
    NotificationCategory Category,
    string Code,
    bool Enabled) : IRequest<Unit>;

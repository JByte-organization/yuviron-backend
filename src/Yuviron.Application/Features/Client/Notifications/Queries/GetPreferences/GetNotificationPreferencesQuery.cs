using MediatR;

namespace Yuviron.Application.Features.Client.Notifications.Queries.GetPreferences;

public sealed record GetNotificationPreferencesQuery : IRequest<NotificationPreferencesDto>;

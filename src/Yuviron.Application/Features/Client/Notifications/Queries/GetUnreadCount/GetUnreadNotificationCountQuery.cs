using MediatR;

namespace Yuviron.Application.Features.Client.Notifications.Queries.GetUnreadCount;

public sealed record GetUnreadNotificationCountQuery() : IRequest<int>;
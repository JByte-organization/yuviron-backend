using MediatR;

namespace Yuviron.Application.Features.Client.Notifications.Commands.MarkAsRead;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest<Unit>;
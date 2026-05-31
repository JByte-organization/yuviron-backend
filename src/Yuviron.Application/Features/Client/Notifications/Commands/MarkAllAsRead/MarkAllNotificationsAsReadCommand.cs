using MediatR;

namespace Yuviron.Application.Features.Client.Notifications.Commands.MarkAllAsRead;

public sealed record MarkAllNotificationsAsReadCommand() : IRequest<Unit>;
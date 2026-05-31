using Yuviron.Application.Common.Models;

namespace Yuviron.Infrastructure.SignalR;

public interface IYuvironClient
{
    Task ReceiveNotification(NotificationDto notification);
}
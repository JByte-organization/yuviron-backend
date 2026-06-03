using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record NewDeviceLoginEvent(
    Guid UserId,
    string DeviceName,
    string BrowserName,
    string IpAddress
) : IDomainEvent;
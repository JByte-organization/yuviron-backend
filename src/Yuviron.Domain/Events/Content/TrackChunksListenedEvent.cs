using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record TrackChunksListenedEvent(
    Guid UserId,
    Guid TrackId,
    string? CountryCode,
    string DeviceType,
    DateTime PlayedAt,
    List<int> StartSeconds, 
    List<int> EndSeconds
) : IDomainEvent; 
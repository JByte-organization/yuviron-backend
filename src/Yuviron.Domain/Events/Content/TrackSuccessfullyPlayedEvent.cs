using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Events;

public record TrackSuccessfullyPlayedEvent(
    Guid UserId,
    Guid TrackId,
    int MsPlayed,
    DateTime PlayedAt,
    PlaybackDeviceType DeviceType,
    PlaybackSourceType SourceType,
    Guid? SourceId
) : IDomainEvent;
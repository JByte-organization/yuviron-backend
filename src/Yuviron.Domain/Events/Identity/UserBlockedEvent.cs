using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record UserBlockedEvent(
    Guid UserId,
    string ReasonCode,
    string ReasonText,
    DateTime? EndsAtUtc
) : IDomainEvent;

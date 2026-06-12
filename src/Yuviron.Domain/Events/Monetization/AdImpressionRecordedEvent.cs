using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record AdImpressionRecordedEvent(
    Guid AdId,
    Guid? UserId,
    DateTime Timestamp,
    string? Context,
    bool IsClicked
) : IDomainEvent;

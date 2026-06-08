using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Events;

public sealed record ComplaintApprovedEvent(
    Guid ComplaintId,
    Guid UserId,
    ComplaintTargetType TargetType,
    Guid TargetId,
    string TargetTitle,
    string? ModerationNote
) : IDomainEvent;

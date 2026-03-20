using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record FileNeedsDeletionEvent(string? FileUrl) : IDomainEvent;
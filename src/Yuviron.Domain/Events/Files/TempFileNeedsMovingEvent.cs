using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record TempFileNeedsMovingEvent(string? TempUrl, string DestinationFolder) : IDomainEvent;
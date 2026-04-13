using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record DirectoryNeedsDeletionEvent(string DirectoryPath) : IDomainEvent;
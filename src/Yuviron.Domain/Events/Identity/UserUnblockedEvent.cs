using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record UserUnblockedEvent(Guid UserId) : IDomainEvent;

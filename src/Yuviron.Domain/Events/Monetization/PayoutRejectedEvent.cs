using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record PayoutRejectedEvent(Guid ArtistId, decimal Amount, string Reason) : IDomainEvent, IArtistEvent;
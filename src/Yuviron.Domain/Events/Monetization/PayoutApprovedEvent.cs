using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record PayoutApprovedEvent(Guid ArtistId, decimal Amount) : IDomainEvent, IArtistEvent;
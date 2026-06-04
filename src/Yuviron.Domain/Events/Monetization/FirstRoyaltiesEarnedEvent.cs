using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record FirstRoyaltiesEarnedEvent(Guid ArtistId) : IDomainEvent, IArtistEvent;
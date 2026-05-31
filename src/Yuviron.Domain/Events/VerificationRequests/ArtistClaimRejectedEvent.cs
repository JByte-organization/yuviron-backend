using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record ArtistClaimRejectedEvent(
    Guid UserId,
    Guid ArtistId,
    string ArtistName,
    string? AdminNote 
) : IDomainEvent;
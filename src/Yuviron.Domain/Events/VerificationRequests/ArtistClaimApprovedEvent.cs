using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record ArtistClaimApprovedEvent(
    Guid UserId,
    Guid ArtistId,
    string ArtistName
) : IDomainEvent;
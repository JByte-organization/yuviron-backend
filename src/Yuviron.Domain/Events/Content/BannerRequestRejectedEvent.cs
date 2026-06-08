using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record BannerRequestRejectedEvent(
    Guid UserId,
    Guid ArtistId,
    string BannerTitle,
    string Reason
) : IDomainEvent;

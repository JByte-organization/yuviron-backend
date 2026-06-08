using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record BannerRequestPaidEvent(
    Guid UserId,
    Guid ArtistId,
    string BannerTitle,
    string PaymentIntentId
) : IDomainEvent;

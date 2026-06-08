using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record BannerRequestApprovedEvent(
    Guid UserId,
    Guid ArtistId,
    string BannerTitle
) : IDomainEvent;

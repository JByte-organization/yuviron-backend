using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record BannerStartedEvent(
    Guid ArtistId,
    Guid BannerId,
    string BannerTitle
) : IDomainEvent, IArtistEvent;

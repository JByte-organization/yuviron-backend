using Yuviron.Domain.Common;


namespace Yuviron.Domain.Events;

public sealed record TrackProcessingCompletedEvent(
    Guid ArtistId, 
    Guid TrackId, 
    string TrackTitle
) : IDomainEvent;
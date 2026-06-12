using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events.External;

public record JamendoTrackSyncRequestedEvent(
    string JamendoId,
    string TrackName,
    Guid ArtistId,
    Guid AlbumId,
    string? AudioDownloadUrl,
    string? CoverUrl,
    string? Isrc,
    int Position,
    List<string> Genres,
    List<string> Moods,
    Guid AdminId
) : IDomainEvent;

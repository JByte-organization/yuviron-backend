using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylistTracks;

public record StudioPlaylistTrackItemDto(
    Guid TrackId,
    string Title,
    IEnumerable<string> ArtistNames,
    string AlbumTitle,
    string? CoverUrl,
    int DurationMs,
    double Position,
    TrackProcessingStatus ProcessingStatus,
    VisibilityStatus VisibilityStatus,
    DateTime AddedAt
);

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistTopTracks;

public record ArtistTopTrackDto(
    Guid Id,
    string Title,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    string? AudioUrl,
    long PlayCount,
    Guid AlbumId,
    string AlbumTitle,
    List<ArtistTopTrackArtistDto> Artists
);

public record ArtistTopTrackArtistDto(
    Guid Id,
    string Name
);
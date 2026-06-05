using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Client.Albums.Queries.GetAlbumById;

public record AlbumDetailsDto(
    Guid Id,
    string Title,
    string? CoverUrl,
    DateTime ReleaseDate,
    int TracksCount,
    IEnumerable<TrackArtistDto> Artists, 
    bool IsSaved = false
);

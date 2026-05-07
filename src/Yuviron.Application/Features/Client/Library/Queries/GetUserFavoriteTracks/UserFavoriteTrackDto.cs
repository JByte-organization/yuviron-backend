using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteTracks;

public record UserFavoriteTrackDto(
    Guid TrackId,
    string Title,
    IEnumerable<TrackArtistDto> ArtistNames,
    Guid AlbumId,
    string AlbumTitle,
    string? CoverUrl,
    int DurationMs,
    DateTime SavedAt
);

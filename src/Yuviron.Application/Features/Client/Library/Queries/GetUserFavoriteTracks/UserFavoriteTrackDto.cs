namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteTracks;

public record UserFavoriteTrackDto(
    Guid TrackId,
    string Title,
    List<string> ArtistNames,
    Guid AlbumId,
    string AlbumTitle,
    string? CoverUrl,
    int DurationMs,
    DateTime SavedAt
);

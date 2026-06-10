namespace Yuviron.Api.Controllers.Admin;

public sealed record CreateAlbumResponse(Guid AlbumId);
public sealed record CreateArtistResponse(Guid ArtistId);
public sealed record CreateGenreResponse(Guid GenreId);
public sealed record CreateMoodResponse(Guid MoodId);
public sealed record CreatePlaylistResponse(Guid PlaylistId);
public sealed record CreateTrackResponse(Guid TrackId);
public sealed record CreateThemeResponse(Guid ThemeId);
public sealed record CreateUserResponse(Guid UserId);

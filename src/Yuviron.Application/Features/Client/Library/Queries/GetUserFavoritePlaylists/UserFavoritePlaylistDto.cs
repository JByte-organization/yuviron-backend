namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoritePlaylists;

public record UserFavoritePlaylistDto(Guid PlaylistId, string Title, string? CoverUrl, DateTime SavedAt);
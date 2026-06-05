namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteAlbums;

public record UserFavoriteAlbumDto(Guid AlbumId, string Title, string? CoverUrl, int ReleaseYear, DateTime SavedAt);
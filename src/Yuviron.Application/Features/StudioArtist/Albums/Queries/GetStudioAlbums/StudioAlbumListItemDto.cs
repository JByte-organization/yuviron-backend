using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.GetStudioAlbums;

public record StudioAlbumListItemDto(
    Guid Id,
    string Title,
    string? CoverUrl,
    ReleaseType ReleaseType,
    VisibilityStatus VisibilityStatus,
    DateTime ReleaseDate,
    DateTime CreatedAt
);
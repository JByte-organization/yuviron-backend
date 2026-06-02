using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.GetStudioAlbums;

public record StudioAlbumListItemDto(
    Guid Id,
    string Title,
    string? CoverUrl,
    int TracksCount,       
    long TotalPlays,       
    DateTime ReleaseDate,
    ReleaseType ReleaseType,
    VisibilityStatus VisibilityStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Queries.DTOs;

public record AlbumListItemDto(
    Guid Id,
    string Title,
    List<string> ArtistNames,
    string? CoverUrl,
    int TracksCount,       
    int TotalPlays,        
    DateTime ReleaseDate,
    VisibilityStatus VisibilityStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
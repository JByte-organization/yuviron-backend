using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Queries.DTOs;

public record AlbumListItemDto(
    Guid Id,
    string Title,
    string? CoverUrl,
    DateTime ReleaseDate,
    VisibilityStatus VisibilityStatus,
    DateTime CreatedAt
);
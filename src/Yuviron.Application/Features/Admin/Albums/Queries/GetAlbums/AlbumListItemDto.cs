using Yuviron.Application.Common.Models;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Queries.DTOs;

public record AlbumListItemDto(
    Guid Id,
    string Title,
    IEnumerable<SimpleArtistDto> Artists,
    string? CoverUrl,
    int TracksCount,       
    long TotalPlays,       
    DateTime ReleaseDate,
    VisibilityStatus VisibilityStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
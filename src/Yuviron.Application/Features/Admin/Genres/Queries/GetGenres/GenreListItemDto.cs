
namespace Yuviron.Application.Features.Admin.Genres.Queries.DTOs;

public record GenreListItemDto(
    Guid Id,
    string? CoverUrl,
    string Name,
    int TracksCount, 
    DateTime CreatedAt,
    DateTime UpdatedAt
);
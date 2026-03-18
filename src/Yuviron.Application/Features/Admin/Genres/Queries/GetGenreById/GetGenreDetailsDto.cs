namespace Yuviron.Application.Features.Admin.Genres.Queries.DTOs;

public record GenreDetailsDto(
    Guid Id,
    string Name,
    string? CoverUrl,
    int TracksCount,    
    DateTime CreatedAt, 
    DateTime UpdatedAt
);
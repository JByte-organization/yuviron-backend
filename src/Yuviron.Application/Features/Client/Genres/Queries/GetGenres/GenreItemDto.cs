namespace Yuviron.Application.Features.Client.Genres.Queries.GetGenres;


public record GenreItemDto(
    Guid Id,
    string Name,
    string? CoverUrl
);
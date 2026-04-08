namespace Yuviron.Application.Features.Admin.Genres.Queries.GetGenresAutocomplete;

public sealed record GenreAutocompleteDto(
    Guid Id,
    string Name,
    string? CoverUrl
);
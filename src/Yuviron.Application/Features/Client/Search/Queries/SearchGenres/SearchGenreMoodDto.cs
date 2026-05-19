namespace Yuviron.Application.Features.Client.Search.Queries.SearchGenres;

public sealed record SearchGenreMoodDto(
    Guid Id,
    string Name,
    string? CoverUrl,
    string Type
);

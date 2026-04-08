namespace Yuviron.Application.Features.Admin.Moods.Queries.GetMoodsAutocomplete;

public sealed record MoodAutocompleteDto(
    Guid Id,
    string Name,
    string? CoverUrl
);
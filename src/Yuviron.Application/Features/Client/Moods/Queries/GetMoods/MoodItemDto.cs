namespace Yuviron.Application.Features.Client.Moods.Queries.GetMoods;

public record MoodItemDto(
    Guid Id,
    string Name,
    string? CoverUrl
);
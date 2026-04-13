
namespace Yuviron.Application.Features.Admin.Moods.Queries.GetMoods;

public sealed record MoodDto(
    Guid Id,
    string Name,
    string? CoverUrl,
    int TracksCount, 
    DateTime CreatedAt,
    DateTime UpdatedAt
);
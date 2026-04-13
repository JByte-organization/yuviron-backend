
namespace Yuviron.Application.Features.Admin.Moods.Queries.DTOs;

public record GetMoodByIdDto(
    Guid Id,
    string Name,
    string? CoverUrl,
    int TracksCount,
    DateTime CreatedAt, 
    DateTime UpdatedAt
);
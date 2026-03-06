using System;

namespace Yuviron.Application.Features.Admin.Moods.Queries.GetMoods;

public sealed record MoodDto(
    Guid Id,
    string Name,
    string? CoverUrl,
    bool IsDeleted,
    DateTime CreatedAt
);
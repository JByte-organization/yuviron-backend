using System;

namespace Yuviron.Application.Features.Admin.Genres.Queries.DTOs;

public record GenreDto(
    Guid Id,
    string Name,
    string? CoverUrl,
    bool IsDeleted,
    DateTime CreatedAt
);
using System;

namespace Yuviron.Application.Features.Admin.Genres.Queries.DTOs;

public record GenreListItemDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
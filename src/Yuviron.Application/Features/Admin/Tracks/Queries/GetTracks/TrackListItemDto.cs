using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;

public record TrackListItemDto(
    Guid Id,
    Guid AlbumId,
    int AlbumPosition,
    string Title,
    int DurationMs,
    bool Explicit,
    VisibilityStatus VisibilityStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
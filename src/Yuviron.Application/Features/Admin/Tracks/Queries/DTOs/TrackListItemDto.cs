using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;

public record TrackListItemDto(
    Guid Id,
    Guid? AlbumId,
    string Title,
    int DurationMs,
    bool Explicit,
    VisibilityStatus VisibilityStatus,
    bool IsDeleted,
    DateTime CreatedAt
);
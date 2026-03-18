using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;

public record TrackListItemDto(
    Guid Id,
    Guid AlbumId,
    string AlbumTitle, 
    int AlbumPosition,
    string Title,
    List<string> ArtistNames, 
    int DurationMs,
    bool Explicit,
    string? CoverUrl, 
    int PlayCount,   
    VisibilityStatus VisibilityStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
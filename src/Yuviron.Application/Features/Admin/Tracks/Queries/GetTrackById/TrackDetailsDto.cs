using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;

public record TrackDetailsDto(
    Guid Id,
    Guid? AlbumId,
    string Title,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    string AudioStorageKey,
    string? PreviewStorageKey,
    VisibilityStatus VisibilityStatus,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<Guid> ArtistIds, 
    List<Guid> GenreIds,
    List<Guid> MoodIds
);
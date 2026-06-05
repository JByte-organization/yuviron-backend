using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylistById;

public record StudioPlaylistDetailsDto(
    Guid Id,
    string Title,
    string? Description,
    string? CoverUrl,
    PlaylistVisibility Visibility,
    int TracksCount,
    long TotalDurationMs,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
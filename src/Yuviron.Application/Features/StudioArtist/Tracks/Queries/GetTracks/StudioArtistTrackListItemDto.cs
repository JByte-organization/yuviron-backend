using Yuviron.Application.Common.Models;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetTracks;

public sealed record StudioArtistTrackListItemDto(
    Guid Id,
    Guid AlbumId,
    string AlbumTitle,
    DateTime ReleaseDate,
    int AlbumPosition,
    string Title,
    List<TrackArtistDto> Artists,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    long PlayCount,
    VisibilityStatus VisibilityStatus,
    TrackProcessingStatus ProcessingStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

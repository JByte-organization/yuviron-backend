using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;

public record TrackArtistSimpleDto(Guid ArtistId, string Name, ArtistRole Role);
public record TrackGenreSimpleDto(Guid GenreId, string Name);
public record TrackMoodSimpleDto(Guid MoodId, string Name);

public record TrackDetailsDto(
    Guid Id,
    Guid AlbumId,
    string AlbumTitle,
    int AlbumPosition,
    string Title,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    string AudioStorageKey,
    string? HlsPlaylistUrl,
    long PlayCount,
    VisibilityStatus VisibilityStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<TrackArtistSimpleDto> Artists,
    List<TrackGenreSimpleDto> Genres,
    List<TrackMoodSimpleDto> Moods 
);
using Yuviron.Application.Common.Models;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTrackById;

public record StudioTrackDetailsDto(
    Guid Id,
    Guid AlbumId,
    string AlbumTitle,
    int AlbumPosition,
    string Title,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    TrackProcessingStatus ProcessingStatus,
    VisibilityStatus VisibilityStatus,
    long TotalPlays,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? LyricsText, 
    IEnumerable<TrackArtistDto> Artists,
    IEnumerable<Guid> GenreIds, 
    IEnumerable<Guid> MoodIds  
);
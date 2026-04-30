
namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackRecommendations;

public record RecommendedTrackDto(
    Guid Id,
    string Title,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    string? AudioUrl,
    List<RecommendedTrackArtistDto> Artists
);

public record RecommendedTrackArtistDto(
    Guid Id,
    string Name
);
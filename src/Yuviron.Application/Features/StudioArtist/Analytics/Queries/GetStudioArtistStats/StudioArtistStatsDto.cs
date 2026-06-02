namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetStudioArtistStats;

public record StudioArtistStatsDto(
    long TotalPlays,
    int MonthlyListeners,
    int TotalAlbums,
    int TotalTracks,
    TopTrackStatDto? TopTrack
);

public record TopTrackStatDto(
    Guid Id,
    string Title,
    string? CoverUrl,
    long PlayCount
);
namespace Yuviron.Application.Features.Client.RecentlyPlayed.Queries.GetUserRecentlyPlayed;

public record RecentlyPlayedTrackDto(
    Guid Id,
    string Title,
    string ArtistNames,
    string? CoverUrl,
    DateTime LastPlayedAt
);
using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Client.RecentlyPlayed.Queries.GetUserRecentlyPlayed;


public record RecentlyPlayedTrackDto(
    Guid Id,
    string Title,
    IEnumerable<SimpleArtistDto> Artists,
    string? CoverUrl,
    DateTime LastPlayedAt
);
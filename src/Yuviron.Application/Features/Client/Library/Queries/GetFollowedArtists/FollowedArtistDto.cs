namespace Yuviron.Application.Features.Client.Library.Queries.GetFollowedArtists;

public sealed record FollowedArtistDto(
    Guid ArtistId,
    string Name,
    string? AvatarUrl,
    int FollowersCount,
    DateTime FollowedAt
);
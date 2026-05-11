namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteArtists;

public sealed record FollowedArtistDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    int FollowersCount
);

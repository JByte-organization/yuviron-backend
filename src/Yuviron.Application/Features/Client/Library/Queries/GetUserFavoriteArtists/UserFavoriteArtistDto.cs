namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteArtists;

public sealed record UserFavoriteArtistDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    int FollowersCount
);

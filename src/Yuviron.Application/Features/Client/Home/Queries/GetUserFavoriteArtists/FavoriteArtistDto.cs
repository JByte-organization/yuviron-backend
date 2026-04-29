
namespace Yuviron.Application.Features.Client.Home.Queries.GetUserFavoriteArtists;

public record FavoriteArtistDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    int FollowersCount
);
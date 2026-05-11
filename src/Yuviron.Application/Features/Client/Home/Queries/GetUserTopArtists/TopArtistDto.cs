
namespace Yuviron.Application.Features.Client.Home.Queries.GetUserTopArtists;

public record TopArtistDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    int FollowersCount
);
using MediatR;

namespace Yuviron.Application.Features.Client.RecentlyPlayed.Queries.GetUserRecentlyPlayed;

public static class GetUserRecentlyPlayedLimits
{
    public const int DefaultLimit = 5;
    public const int MaxLimit = 5;
}

public record GetUserRecentlyPlayedQuery(
    int Limit = GetUserRecentlyPlayedLimits.DefaultLimit
) : IRequest<List<RecentlyPlayedTrackDto>>;

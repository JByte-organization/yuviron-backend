using MediatR;

namespace Yuviron.Application.Features.Client.RecentlyPlayed.Queries.GetUserRecentlyPlayed;

public record GetUserRecentlyPlayedQuery(int Limit = 5) : IRequest<List<RecentlyPlayedTrackDto>>;
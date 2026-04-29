using MediatR;

namespace Yuviron.Application.Features.Client.Home.Queries.GetUserTopTracks;

public record GetUserTopTracksQuery(int Limit = 5) : IRequest<List<TopTrackDto>>;
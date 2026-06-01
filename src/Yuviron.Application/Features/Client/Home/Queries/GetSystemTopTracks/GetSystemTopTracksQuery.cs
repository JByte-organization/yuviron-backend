using MediatR;
using Yuviron.Application.Features.Client.Home.Queries.GetUserTopTracks; 

namespace Yuviron.Application.Features.Client.Home.Queries.GetSystemTopTracks;

public record GetSystemTopTracksQuery(int Limit = 5) : IRequest<List<TopTrackDto>>;
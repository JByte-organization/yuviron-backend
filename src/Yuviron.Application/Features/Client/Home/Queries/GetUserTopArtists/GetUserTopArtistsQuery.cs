using MediatR;

namespace Yuviron.Application.Features.Client.Home.Queries.GetUserTopArtists;

public record GetUserTopArtistsQuery(int Limit = 5) : IRequest<List<TopArtistDto>>;
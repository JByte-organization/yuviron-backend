using MediatR;
using Yuviron.Application.Features.Client.Home.Queries.GetUserTopArtists;

namespace Yuviron.Application.Features.Client.Home.Queries.GetSystemTopArtists;

public record GetSystemTopArtistsQuery(int Limit = 5) : IRequest<List<TopArtistDto>>;
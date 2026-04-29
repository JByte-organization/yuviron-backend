using MediatR;

namespace Yuviron.Application.Features.Client.Home.Queries.GetUserFavoriteArtists;

public record GetUserFavoriteArtistsQuery(int Limit = 5) : IRequest<List<FavoriteArtistDto>>;
using MediatR;

namespace Yuviron.Application.Features.Client.Library.Commands.RemoveAlbumFromFavorites;

public sealed record RemoveAlbumFromFavoritesCommand(Guid AlbumId) : IRequest<Unit>;
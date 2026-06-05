using MediatR;

namespace Yuviron.Application.Features.Client.Library.Commands.AddAlbumToFavorites;

public sealed record AddAlbumToFavoritesCommand(Guid AlbumId) : IRequest<Unit>;
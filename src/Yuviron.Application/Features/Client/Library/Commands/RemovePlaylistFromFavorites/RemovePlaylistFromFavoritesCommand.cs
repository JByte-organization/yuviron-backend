using MediatR;

namespace Yuviron.Application.Features.Client.Library.Commands.RemovePlaylistFromFavorites;

public sealed record RemovePlaylistFromFavoritesCommand(Guid PlaylistId) : IRequest<Unit>;
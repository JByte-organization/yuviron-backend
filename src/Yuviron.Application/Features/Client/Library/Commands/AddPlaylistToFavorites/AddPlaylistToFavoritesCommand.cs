using MediatR;

namespace Yuviron.Application.Features.Client.Library.Commands.AddPlaylistToFavorites;

public sealed record AddPlaylistToFavoritesCommand(Guid PlaylistId) : IRequest<Unit>;
using MediatR;

namespace Yuviron.Application.Features.Client.Library.Commands.RemoveTrackFromFavorites;

public sealed record RemoveTrackFromFavoritesCommand(Guid TrackId) : IRequest<Unit>;

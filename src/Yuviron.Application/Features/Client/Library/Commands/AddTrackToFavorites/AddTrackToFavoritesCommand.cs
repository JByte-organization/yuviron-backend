using MediatR;

namespace Yuviron.Application.Features.Client.Library.Commands.AddTrackToFavorites;

public sealed record AddTrackToFavoritesCommand(Guid TrackId) : IRequest<Unit>;

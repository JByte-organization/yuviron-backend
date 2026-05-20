using MediatR;

namespace Yuviron.Application.Features.Client.Artists.Commands.UnfollowArtist;

public sealed record UnfollowArtistCommand(Guid ArtistId) : IRequest<Unit>;
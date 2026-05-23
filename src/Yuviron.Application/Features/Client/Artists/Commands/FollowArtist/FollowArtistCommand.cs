using MediatR;

namespace Yuviron.Application.Features.Client.Artists.Commands.FollowArtist;

public sealed record FollowArtistCommand(Guid ArtistId) : IRequest<Unit>;
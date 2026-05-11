using MediatR;

namespace Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistById;

public record GetPlaylistByIdQuery(Guid Id) : IRequest<PlaylistDetailsClientDto>;
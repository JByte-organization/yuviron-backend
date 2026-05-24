using MediatR;

namespace Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistById;

//это огромная красивая шапка,
//которая появляется в центре экрана,
//когда ты нажимаешь на любой плейлист,
//чтобы посмотреть, кто его создал и сколько часов музыки в нем собрано.
public record GetPlaylistByIdQuery(Guid Id) : IRequest<PlaylistDetailsClientDto>;
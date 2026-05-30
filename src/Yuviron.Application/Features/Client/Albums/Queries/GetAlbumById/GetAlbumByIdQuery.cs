using MediatR;

namespace Yuviron.Application.Features.Client.Albums.Queries.GetAlbumById;

public sealed record GetAlbumByIdQuery(Guid AlbumId) : IRequest<AlbumDetailsDto>;

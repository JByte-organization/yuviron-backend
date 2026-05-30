using MediatR;

namespace Yuviron.Application.Features.Client.Albums.Queries.GetAlbumTracks;

public sealed record GetAlbumTracksQuery(Guid AlbumId) : IRequest<List<AlbumTrackItemDto>>;

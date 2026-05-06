using MediatR;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistPopularReleases;

public sealed record GetArtistPopularReleasesQuery(
    Guid ArtistId,
    int Limit = 10
) : IRequest<List<ArtistAlbumDto>>;

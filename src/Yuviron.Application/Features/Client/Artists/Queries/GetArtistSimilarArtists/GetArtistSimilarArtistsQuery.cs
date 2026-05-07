using MediatR;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistSimilarArtists;

public sealed record GetArtistSimilarArtistsQuery(
    Guid ArtistId,
    int Limit = 10
) : IRequest<List<SimilarArtistDto>>;

using MediatR;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistRelatedTracks;

public sealed record GetArtistRelatedTracksQuery(
    Guid ArtistId,
    int Limit = 10
) : IRequest<List<RelatedTrackDto>>;

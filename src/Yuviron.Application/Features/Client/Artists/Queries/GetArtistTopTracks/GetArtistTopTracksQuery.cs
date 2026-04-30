using MediatR;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistTopTracks;

public record GetArtistTopTracksQuery(Guid ArtistId, int Limit = 10) : IRequest<List<ArtistTopTrackDto>>;
using MediatR;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistById;

public sealed record GetArtistByIdQuery(Guid ArtistId) : IRequest<ArtistDetailsDto>;

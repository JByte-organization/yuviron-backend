using MediatR;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackById;

public record GetTrackByIdQuery(Guid Id) : IRequest<TrackDetailsDto>;
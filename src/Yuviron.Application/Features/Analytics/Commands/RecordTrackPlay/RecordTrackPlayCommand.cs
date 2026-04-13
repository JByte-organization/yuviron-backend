using MediatR;

namespace Yuviron.Application.Features.Analytics.Commands.RecordTrackPlay;

public record RecordTrackPlayCommand(Guid TrackId, Guid ArtistId) : IRequest<Unit>;
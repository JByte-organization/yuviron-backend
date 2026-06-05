using System;
using MediatR;

namespace Yuviron.Application.Features.Analytics.Commands.StartTrackPlay;

public record StartPlayRequest(Guid TrackId);

public record StartPlayResponse(Guid PlaySessionId);

public record StartTrackPlayCommand(Guid TrackId, Guid UserId) : IRequest<Guid>;
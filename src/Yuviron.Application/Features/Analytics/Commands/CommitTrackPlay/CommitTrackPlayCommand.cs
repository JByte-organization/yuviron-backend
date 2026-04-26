using System;
using MediatR;

namespace Yuviron.Application.Features.Analytics.Commands.CommitTrackPlay;

public record CommitPlayRequest(Guid PlaySessionId, Guid TrackId, Guid ArtistId);

public record CommitTrackPlayCommand(
    Guid PlaySessionId, 
    Guid TrackId, 
    Guid ArtistId, 
    Guid UserId) : IRequest<Unit>;
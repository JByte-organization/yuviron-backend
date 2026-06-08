using MediatR;
using System;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackStreamUrl;

public record GetTrackStreamUrlQuery(
    Guid TrackId
) : IRequest<TrackStreamUrlResponse>;
using MediatR;
using System;
using System.Collections.Generic;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackPlaysOverTime;

public sealed record GetTrackPlaysOverTimeQuery(Guid ArtistId, Guid TrackId, int Days = 30) : IRequest<List<PlaysOverTimePointDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}
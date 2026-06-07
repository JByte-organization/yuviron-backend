using MediatR;
using System;
using System.Collections.Generic;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackPlaysOverTime;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetArtistPlaysOverTime;

public sealed record GetArtistPlaysOverTimeQuery(Guid ArtistId, int Days = 30) : IRequest<List<PlaysOverTimePointDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}
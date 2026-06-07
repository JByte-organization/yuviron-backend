using MediatR;
using System;
using System.Collections.Generic;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackRetention;

public sealed record GetTrackRetentionQuery(Guid ArtistId, Guid TrackId) : IRequest<List<TrackRetentionPointDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}
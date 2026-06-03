using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetStudioArtistStats;

public sealed record GetStudioArtistStatsQuery(Guid ArtistId) : IRequest<ArtistAnalyticsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}
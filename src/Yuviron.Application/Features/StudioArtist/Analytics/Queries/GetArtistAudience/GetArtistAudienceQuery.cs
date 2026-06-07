using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetArtistAudience;

public sealed record GetArtistAudienceQuery(Guid ArtistId, int Days = 30) : IRequest<ArtistAudienceDashboardDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}
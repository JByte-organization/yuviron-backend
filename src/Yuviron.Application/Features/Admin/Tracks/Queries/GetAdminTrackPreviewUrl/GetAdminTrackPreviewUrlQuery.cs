using System;
using MediatR;
using Yuviron.Domain.Enums;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetAdminTrackPreviewUrl;

public sealed record GetAdminTrackPreviewUrlQuery(Guid TrackId) : IRequest<AdminTrackPreviewUrlResponse>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}

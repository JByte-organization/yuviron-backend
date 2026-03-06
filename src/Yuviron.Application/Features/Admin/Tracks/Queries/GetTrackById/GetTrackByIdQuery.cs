using System;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTrackById;

public sealed record GetTrackByIdQuery(Guid TrackId) : IRequest<TrackDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
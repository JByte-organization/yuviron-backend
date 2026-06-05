using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTrackById;

public sealed record GetStudioTrackByIdQuery(Guid TrackId) 
    : IRequest<StudioTrackDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}
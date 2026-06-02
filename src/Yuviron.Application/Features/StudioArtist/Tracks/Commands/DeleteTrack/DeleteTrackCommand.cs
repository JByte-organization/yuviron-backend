using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteTrack;

public sealed record DeleteTrackCommand(Guid TrackId) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
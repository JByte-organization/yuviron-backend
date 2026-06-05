using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.SetArtistPin;

public sealed record SetArtistPinCommand(
    Guid ArtistId,
    ArtistPinType EntityType,
    Guid EntityId,
    int Position = 1 
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
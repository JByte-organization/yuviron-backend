using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.RemoveArtistPin;

public sealed record RemoveArtistPinCommand(
    Guid ArtistId, 
    int Position = 1
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
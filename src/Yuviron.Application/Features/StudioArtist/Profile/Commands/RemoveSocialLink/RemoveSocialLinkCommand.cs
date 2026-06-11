using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.RemoveSocialLink;

public sealed record RemoveSocialLinkCommand(
    Guid ArtistId, 
    SocialLinkType Type
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
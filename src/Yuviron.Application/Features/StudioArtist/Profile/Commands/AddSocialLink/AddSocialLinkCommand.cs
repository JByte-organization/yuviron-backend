using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.AddSocialLink;

public sealed record AddSocialLinkRequest(SocialLinkType Type, string Url);

public sealed record AddSocialLinkCommand(
    Guid ArtistId,
    SocialLinkType Type,
    string Url
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
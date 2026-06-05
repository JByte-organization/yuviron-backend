using MediatR;
using System;
using System.Collections.Generic;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.UpdateSocialLinks;

public sealed record SocialLinkItem(string Type, string Url);

public sealed record UpdateSocialLinksRequest(List<SocialLinkItem> Links);

public sealed record UpdateSocialLinksCommand(
    Guid ArtistId,
    List<SocialLinkItem> Links
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
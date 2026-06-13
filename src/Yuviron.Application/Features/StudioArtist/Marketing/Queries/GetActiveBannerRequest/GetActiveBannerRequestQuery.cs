using Yuviron.Domain.Enums;
﻿using MediatR;
using System;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.StudioArtist.Marketing.Queries.GetActiveBannerRequest;

public record GetActiveBannerRequestQuery(Guid ArtistId) : IRequest<ActiveBannerRequestDto?>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}

using Yuviron.Domain.Enums;
﻿using MediatR;
using System;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBannerRequestById;

public record GetBannerRequestByIdQuery(Guid RequestId) : IRequest<BannerRequestDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}

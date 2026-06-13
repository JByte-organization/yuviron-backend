using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Banners.Commands.ApproveBannerRequest;

public sealed record ApproveBannerRequestCommand(
    Guid RequestId,
    bool IsActive = true,
    DateTime? StartsAtUtc = null,
    DateTime? EndsAtUtc = null,
    string? TargetCountries = null,
    string? TargetGenres = null
) : IRequest<Guid>, ISecuredRequest 
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}

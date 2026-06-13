using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;
using System;

namespace Yuviron.Application.Features.Admin.Banners.Commands.UpdateBanner;

public record UpdateBannerCommand(
    Guid BannerId,
    string Title,
    Guid? BannerFileId,
    string TargetUrl,
    bool IsActive,
    Guid? ArtistId = null,
    DateTime? StartsAtUtc = null,
    DateTime? EndsAtUtc = null,
    string? TargetCountries = null,
    string? TargetGenres = null
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}

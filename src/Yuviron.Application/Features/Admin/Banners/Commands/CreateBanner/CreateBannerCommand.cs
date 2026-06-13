using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;
using System;

namespace Yuviron.Application.Features.Admin.Banners.Commands.CreateBanner;

public record CreateBannerCommand(
    string Title,
    Guid BannerFileId,
    string TargetUrl,
    bool IsActive,
    Guid? ArtistId = null,
    DateTime? StartsAtUtc = null,
    DateTime? EndsAtUtc = null,
    string? TargetCountries = null,
    string? TargetGenres = null
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}

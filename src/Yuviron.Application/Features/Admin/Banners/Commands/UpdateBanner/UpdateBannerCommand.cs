using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Banners.Commands.UpdateBanner;

public sealed record UpdateBannerCommand(
    Guid BannerId,
    string Title,
    Guid? BannerFileId,
    string TargetUrl,
    int SortOrder,
    bool IsActive
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Banners.Commands.CreateBanner;

public record CreateBannerCommand(
    string Title,
    Guid BannerFileId,
    string TargetUrl,
    int SortOrder,
    bool IsActive
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}
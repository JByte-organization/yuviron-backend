using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Banners.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBanners;

public sealed record GetBannersQuery(
    string? SearchTerm,
    int Page = 1,
    int PageSize = 50 
) : PaginatedQuery(SearchTerm, Page, PageSize), 
    IRequest<PaginatedList<BannerListItemDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
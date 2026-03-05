using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbums;

public sealed record GetAlbumsQuery(
    string? SearchTerm, 
    VisibilityStatus? Status, 
    bool IncludeDeleted, 
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedList<AlbumListItemDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}
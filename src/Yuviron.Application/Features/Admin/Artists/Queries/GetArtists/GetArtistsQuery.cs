using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtists;

public sealed record GetArtistsQuery(
    string? SearchTerm, 
    VerificationStatus? VerificationStatus, 
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, Page, PageSize),
    IRequest<PaginatedList<ArtistListItemDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
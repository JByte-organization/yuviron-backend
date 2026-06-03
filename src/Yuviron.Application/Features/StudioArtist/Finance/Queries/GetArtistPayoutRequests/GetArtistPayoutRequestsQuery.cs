using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Finance.Queries.GetArtistPayoutRequests;

public sealed record GetArtistPayoutRequestsQuery(
    Guid ArtistId,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, null, null, Page, PageSize), 
    IRequest<PaginatedList<ArtistPayoutRequestDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}
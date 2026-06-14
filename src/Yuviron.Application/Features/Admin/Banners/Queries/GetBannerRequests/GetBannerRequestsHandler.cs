using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBannerRequests;

public sealed class GetBannerRequestsHandler : IRequestHandler<GetBannerRequestsQuery, PaginatedList<BannerRequestListItemDto>>
{
    private readonly IContentContext _contentContext;

    public GetBannerRequestsHandler(IContentContext contentContext)
    {
        _contentContext = contentContext;
    }

    public async Task<PaginatedList<BannerRequestListItemDto>> Handle(GetBannerRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _contentContext.BannerRequests
            .AsNoTracking()
            .Include(br => br.Artist)
            .Include(br => br.Album)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(br => br.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(br => br.Title.Contains(request.SearchTerm) || br.Artist.Name.Contains(request.SearchTerm));

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: "CreatedAt",
            defaultDesc: true);

        var projectedQuery = sortedQuery.Select(br => new BannerRequestListItemDto(
            br.Id,
            br.ArtistId,
            br.Artist.Name,
            br.AlbumId,
            br.Album != null ? br.Album.Title : null,
            br.Title,
            br.BannerUrl,
            br.Status,
            br.AdminNotes,
            br.CreatedAt,
            br.UpdatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
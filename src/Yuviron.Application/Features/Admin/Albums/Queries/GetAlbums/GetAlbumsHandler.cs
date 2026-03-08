using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions; // Тут лежит наш QueryableExtensions
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbums;

public sealed class GetAlbumsHandler : IRequestHandler<GetAlbumsQuery, PaginatedList<AlbumListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAlbumsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AlbumListItemDto>> Handle(GetAlbumsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Albums.AsNoTracking();

        if (request.IncludeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(a => a.Title.StartsWith(request.SearchTerm));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.VisibilityStatus == request.Status.Value);
        }

        var projectedQuery = query
            .OrderByDescending(a => a.CreatedAt) 
            .Select(a => new AlbumListItemDto(
                a.Id,
                a.Title,
                a.CoverUrl,
                a.ReleaseDate,
                a.VisibilityStatus,
                a.IsDeleted,
                a.CreatedAt
            ));

        // 4. Вся магия пагинации в одной строке!
        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
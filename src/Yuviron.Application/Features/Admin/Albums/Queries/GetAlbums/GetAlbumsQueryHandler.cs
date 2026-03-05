using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbums;

public sealed class GetAlbumsQueryHandler : IRequestHandler<GetAlbumsQuery, PaginatedList<AlbumListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAlbumsQueryHandler(IApplicationDbContext context)
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
            query = query.Where(a => a.Title.Contains(request.SearchTerm)); 
        }

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.VisibilityStatus == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt) 
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AlbumListItemDto(
                a.Id,
                a.Title,
                a.CoverUrl,
                a.ReleaseDate,
                a.VisibilityStatus,
                a.IsDeleted,
                a.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<AlbumListItemDto>(items, totalCount, request.Page, request.PageSize);
    }
}
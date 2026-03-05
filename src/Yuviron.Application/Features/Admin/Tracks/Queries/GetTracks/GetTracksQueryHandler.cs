using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs;
using Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTracks;

public sealed class GetTracksQueryHandler : IRequestHandler<GetTracksQuery, PaginatedList<TrackListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTracksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TrackListItemDto>> Handle(GetTracksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tracks.AsNoTracking();

        if (request.IncludeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(t => t.Title.Contains(request.SearchTerm));
        }

        if (request.AlbumId.HasValue)
        {
            query = query.Where(t => t.AlbumId == request.AlbumId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.VisibilityStatus == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TrackListItemDto(
                t.Id,
                t.AlbumId,
                t.Title,
                t.DurationMs,
                t.Explicit,
                t.VisibilityStatus,
                t.IsDeleted,
                t.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<TrackListItemDto>(items, totalCount, request.Page, request.PageSize);
    }
}
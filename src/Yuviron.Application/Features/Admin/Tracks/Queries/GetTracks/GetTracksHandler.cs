using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTracks;

public sealed class GetTracksHandler : IRequestHandler<GetTracksQuery, PaginatedList<TrackListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTracksHandler(IApplicationDbContext context)
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

        var projectedQuery = query
            .OrderBy(t => t.AlbumId)
            .ThenBy(t => t.AlbumPosition)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => new TrackListItemDto(
                t.Id,
                t.AlbumId,
                t.AlbumPosition,
                t.Title,
                t.DurationMs,
                t.Explicit,
                t.VisibilityStatus,
                t.CreatedAt,
                t.UpdatedAt
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
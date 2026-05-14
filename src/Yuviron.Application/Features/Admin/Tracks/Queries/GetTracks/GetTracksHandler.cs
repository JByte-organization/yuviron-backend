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
        var query = _context.Tracks
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(t => t.Title.Contains(request.SearchTerm));

        if (request.AlbumId.HasValue)
            query = query.Where(t => t.AlbumId == request.AlbumId.Value);

        if (request.Status.HasValue)
            query = query.Where(t => t.VisibilityStatus == request.Status.Value);

        var projectedQuery = query
            .Select(t => new TrackListItemDto(
                t.Id,
                t.AlbumId,
                t.Album != null ? t.Album.Title : "Unknown Album",
                t.AlbumPosition,
                t.Title,
                t.TrackArtists.Select(ta => ta.Artist.Name).ToList(), 
                t.DurationMs,
                t.Explicit,
                t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                t.PlayCount,
                t.VisibilityStatus,
                t.CreatedAt,
                t.UpdatedAt
            ));

        var sortedQuery = projectedQuery.ApplySorting(request.SortBy, request.SortOrder);

        return await sortedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
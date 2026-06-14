using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTracks;

public sealed class GetTracksHandler : IRequestHandler<GetTracksQuery, PaginatedList<TrackListItemDto>>
{
    private readonly ICatalogContext _catalogContext;

    public GetTracksHandler(ICatalogContext catalogContext)
    {
        _catalogContext = catalogContext;
    }

    public async Task<PaginatedList<TrackListItemDto>> Handle(GetTracksQuery request, CancellationToken cancellationToken)
    {
        var query = _catalogContext.Tracks
            .AsNoTracking()
            ;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(t => t.Title.Contains(request.SearchTerm));

        if (request.AlbumId.HasValue)
            query = query.Where(t => t.AlbumId == request.AlbumId.Value);

        if (request.Status.HasValue)
            query = query.Where(t => t.VisibilityStatus == request.Status.Value);

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(Track.CreatedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<Track, object>>>
            {
                ["AlbumTitle"] = t => t.Album != null ? t.Album.Title : "Unknown Album",

                ["ArtistNames"] = t => t.TrackArtists
                    
                    .OrderBy(ta => ta.Role == ArtistRole.Main ? 0 : 1)
                    .Select(ta => ta.Artist.Name)
                    .FirstOrDefault()!
            });

        var projectedQuery = sortedQuery.Select(t => new TrackListItemDto(
            t.Id,
            t.AlbumId,
            t.Album != null ? t.Album.Title : "Unknown Album",
            t.AlbumPosition,
            t.Title,
            t.TrackArtists
                
                .OrderBy(ta => ta.Role == ArtistRole.Main ? 0 : 1)
                .Select(ta => ta.Artist.Name)
                .ToList(), 
            t.DurationMs,
            t.Explicit,
            t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
            t.PlayCount,
            t.VisibilityStatus,
            t.CreatedAt,
            t.UpdatedAt
        ));

        // 5. ПАГИНАЦИЯ
        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}

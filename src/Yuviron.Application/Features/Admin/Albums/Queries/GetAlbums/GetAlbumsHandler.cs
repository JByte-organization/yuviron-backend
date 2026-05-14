using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
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
        var query = _context.Albums
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(a => a.Title.Contains(request.SearchTerm));

        if (request.Status.HasValue)
            query = query.Where(a => a.VisibilityStatus == request.Status.Value);

        var projectedQuery = query
            .Select(a => new AlbumListItemDto(
                a.Id,
                a.Title,
                a.AlbumArtists.Select(aa => new SimpleArtistDto(aa.ArtistId, aa.Artist.Name)), 
                a.CoverUrl,
                a.Tracks.Count,                 
                a.Tracks.Sum(t => t.PlayCount),   
                a.ReleaseDate,
                a.ReleaseType,
                a.VisibilityStatus,
                a.CreatedAt,
                a.UpdatedAt
            ));

        var sortedQuery = projectedQuery.ApplySorting(
            request.SortBy, 
            request.SortOrder, 
            defaultSortBy: nameof(AlbumListItemDto.CreatedAt), 
            defaultDesc: true);

        return await sortedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
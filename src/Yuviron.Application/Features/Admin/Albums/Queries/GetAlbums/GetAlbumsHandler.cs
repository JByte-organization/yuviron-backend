using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbums;

public sealed class GetAlbumsHandler : IRequestHandler<GetAlbumsQuery, PaginatedList<AlbumListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAlbumsHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<AlbumListItemDto>> Handle(GetAlbumsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Albums
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(a => a.Title.Contains(request.SearchTerm));

        if (request.Status.HasValue)
            query = query.Where(a => a.VisibilityStatus == request.Status.Value);

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(Album.CreatedAt),
            mapping: new Dictionary<string, Expression<Func<Album, object>>>
            {
                [nameof(AlbumListItemDto.TracksCount)] = a => a.Tracks.Count(t => !t.IsDeleted),
                [nameof(AlbumListItemDto.TotalPlays)] = a => a.Tracks.Where(t => !t.IsDeleted).Sum(t => t.PlayCount)
            });

        var projectedQuery = sortedQuery.Select(a => new AlbumListItemDto(
            a.Id,
            a.Title,
            a.AlbumArtists
                .Where(aa => !aa.Artist.IsDeleted)
                .Select(aa => new SimpleArtistDto(aa.ArtistId, aa.Artist.Name)), 
            a.CoverUrl,
            a.Tracks.Count(t => !t.IsDeleted),                 
            a.Tracks.Where(t => !t.IsDeleted).Sum(t => t.PlayCount),   
            a.ReleaseDate,
            a.ReleaseType,
            a.VisibilityStatus,
            a.CreatedAt,
            a.UpdatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}

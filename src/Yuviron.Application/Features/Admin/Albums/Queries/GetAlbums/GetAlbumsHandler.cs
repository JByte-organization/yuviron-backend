using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
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
        var query = _context.Albums.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(a => a.Title.Contains(request.SearchTerm));

        if (request.Status.HasValue)
            query = query.Where(a => a.VisibilityStatus == request.Status.Value);

        var projectedQuery = query
            .OrderByDescending(a => a.CreatedAt) 
            .Select(a => new AlbumListItemDto(
                a.Id,
                a.Title,
                a.AlbumArtists.Select(aa => aa.Artist.Name).ToList(), 
                a.CoverUrl,
                a.Tracks.Count,                 
                a.Tracks.Sum(t => t.PlayCount),   
                a.ReleaseDate,
                a.VisibilityStatus,
                a.CreatedAt,
                a.UpdatedAt
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
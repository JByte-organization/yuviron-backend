using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;

public sealed class GetArtistAlbumsHandler : IRequestHandler<GetArtistAlbumsQuery, PaginatedList<ArtistAlbumDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtistAlbumsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ArtistAlbumDto>> Handle(GetArtistAlbumsQuery request, CancellationToken cancellationToken)
    {
        bool artistExists = await _context.Artists
            .AnyAsync(a => a.Id == request.ArtistId && !a.IsDeleted, cancellationToken);

        if (!artistExists)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        var query = _context.Albums
            .AsNoTracking()
            .Where(a => !a.IsDeleted 
                     && a.VisibilityStatus == VisibilityStatus.Published
                     && a.AlbumArtists.Any(aa => aa.ArtistId == request.ArtistId))
            .OrderByDescending(a => a.ReleaseDate); 

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = (IOrderedQueryable<Album>)query.Where(a => a.Title.Contains(request.SearchTerm));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new ArtistAlbumDto(
                a.Id,
                a.Title,
                a.CoverUrl,
                a.ReleaseDate.Year 
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<ArtistAlbumDto>(items, totalCount, request.Page, request.PageSize);
    }
}
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;
using Yuviron.Application.Extensions; // <-- Подключили экстеншены

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;

public sealed class GetArtistAlbumsHandler : IRequestHandler<GetArtistAlbumsQuery, PaginatedList<ArtistAlbumDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider; // <-- Добавили TimeProvider

    public GetArtistAlbumsHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<ArtistAlbumDto>> Handle(GetArtistAlbumsQuery request, CancellationToken cancellationToken)
    {
        bool artistExists = await _context.Artists
            .AnyAsync(a => a.Id == request.ArtistId && !a.IsDeleted, cancellationToken);

        if (!artistExists) throw new NotFoundException(nameof(Artist), request.ArtistId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        IQueryable<Album> query = _context.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == request.ArtistId));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(a => a.Title.Contains(request.SearchTerm));
        }

        var projectedQuery = query
            .OrderByDescending(a => a.ReleaseDate)
            .Select(a => new ArtistAlbumDto(
                a.Id,
                a.Title,
                a.CoverUrl,
                a.ReleaseDate.Year 
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
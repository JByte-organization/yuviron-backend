using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;

public sealed class GetArtistAlbumsHandler : IRequestHandler<GetArtistAlbumsQuery, PaginatedList<ArtistAlbumDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetArtistAlbumsHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<ArtistAlbumDto>> Handle(GetArtistAlbumsQuery request, CancellationToken cancellationToken)
    {
        var artistExists = await _context.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArtistId && !a.IsDeleted, cancellationToken);

        if (!artistExists)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var query = _context.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .ForArtist(request.ArtistId)
            .Where(a => a.ReleaseType == ReleaseType.Album);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(a => a.Title.Contains(request.SearchTerm));
        }

        var projectedQuery = query
            .OrderByDescending(a => a.ReleaseDate)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new ArtistAlbumDto(
                a.Id,
                a.Title,
                a.CoverUrl,
                a.ReleaseDate.Year
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}

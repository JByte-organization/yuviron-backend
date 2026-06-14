using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistSingles;

public sealed class GetArtistSinglesHandler : IRequestHandler<GetArtistSinglesQuery, PaginatedList<ArtistAlbumDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public GetArtistSinglesHandler(ICatalogContext catalogContext, TimeProvider timeProvider,
        ICacheService cache,
        ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ArtistAlbumDto>> Handle(GetArtistSinglesQuery request, CancellationToken cancellationToken)
    {
        var artistExists = await _catalogContext.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArtistId , cancellationToken);

        if (!artistExists)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var query = _catalogContext.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .ForArtistMain(request.ArtistId)
            .Where(a => a.ReleaseType == ReleaseType.Single);

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder,
            defaultSortBy: nameof(Album.ReleaseDate),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<Album, object>>>
            {
                ["ReleaseYear"] = a => a.ReleaseDate
            });

        var projectedQuery = sortedQuery.Select(a => new ArtistAlbumDto(
            a.Id,
            a.Title,
            a.CoverUrl,
            a.ReleaseDate.Year,
            false // <-- ЯВНЫЙ FALSE
        ));

        var result = await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);

        return await result.EnrichWithCacheAsync(
            _cache, 
            _currentUser.UserId, 
            "saved_albums", 
            x => x.Id, 
            (x, saved) => x with { IsSaved = saved }, 
            cancellationToken);
    }
}

using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;

public sealed class GetArtistAlbumsHandler : IRequestHandler<GetArtistAlbumsQuery, PaginatedList<ArtistAlbumDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public GetArtistAlbumsHandler(
        ICatalogContext catalogContext, 
        TimeProvider timeProvider,
        ICacheService cache,
        ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ArtistAlbumDto>> Handle(GetArtistAlbumsQuery request, CancellationToken cancellationToken)
    {
        var artistExists = await _catalogContext.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArtistId , cancellationToken);

        if (!artistExists) throw new NotFoundException(nameof(Artist), request.ArtistId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var query = _catalogContext.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .ForArtist(request.ArtistId)
            .Where(a => a.ReleaseType == ReleaseType.Album)
            .Where(a => a.Tracks.Any(t => !t.IsDeleted 
                                          && t.VisibilityStatus == VisibilityStatus.Published 
                                          && t.ProcessingStatus == TrackProcessingStatus.Ready));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(a => a.Title.Contains(request.SearchTerm));

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
            false 
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
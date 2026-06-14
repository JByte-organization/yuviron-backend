using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.GetStudioAlbums;

public sealed class GetStudioAlbumsHandler : IRequestHandler<GetStudioAlbumsQuery, PaginatedList<StudioAlbumListItemDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;

    public GetStudioAlbumsHandler(ICatalogContext catalogContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<StudioAlbumListItemDto>> Handle(GetStudioAlbumsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasViewerAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to this artist's content.");

        var query = _catalogContext.Albums.AsNoTracking()
            .Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == request.ArtistId) && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(a => a.Title.Contains(request.SearchTerm));

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(Album.CreatedAt),
            defaultDesc: true);

        var projectedQuery = sortedQuery.Select(a => new StudioAlbumListItemDto(
            a.Id,
            a.Title,
            a.CoverUrl,
            a.ReleaseType,
            a.VisibilityStatus,
            a.ReleaseDate,
            a.CreatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}

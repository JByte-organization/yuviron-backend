using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteAlbums;

public sealed class GetUserFavoriteAlbumsHandler : IRequestHandler<GetUserFavoriteAlbumsQuery, PaginatedList<UserFavoriteAlbumDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public GetUserFavoriteAlbumsHandler(IApplicationDbContext context, ICurrentUserService currentUserService, TimeProvider timeProvider)
    {
        _context = context; _currentUserService = currentUserService; _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<UserFavoriteAlbumDto>> Handle(GetUserFavoriteAlbumsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var query = _context.UserSavedAlbums.AsNoTracking()
            .Where(usa => usa.UserId == userId && _context.Albums.AvailableForPublic(utcNow).Any(a => a.Id == usa.AlbumId));

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder, 
            defaultSortBy: nameof(UserSavedAlbum.SavedAt), 
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<UserSavedAlbum, object>>>
            {
                ["Title"] = usa => usa.Album.Title,
                ["ReleaseYear"] = usa => usa.Album.ReleaseDate, 
                ["SavedAt"] = usa => usa.SavedAt
            });

        var projectedQuery = sortedQuery.Select(usa => new UserFavoriteAlbumDto(
            usa.AlbumId,
            usa.Album.Title,
            usa.Album.CoverUrl,
            usa.Album.ReleaseDate.Year,
            usa.SavedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
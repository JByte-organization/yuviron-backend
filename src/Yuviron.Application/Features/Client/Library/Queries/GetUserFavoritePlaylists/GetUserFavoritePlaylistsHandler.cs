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
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoritePlaylists;

public sealed class GetUserFavoritePlaylistsHandler : IRequestHandler<GetUserFavoritePlaylistsQuery, PaginatedList<UserFavoritePlaylistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserFavoritePlaylistsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context; _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<UserFavoritePlaylistDto>> Handle(GetUserFavoritePlaylistsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var query = _context.UserSavedPlaylists.AsNoTracking()
            .Where(usp => usp.UserId == userId && _context.Playlists.Any(p => p.Id == usp.PlaylistId && !p.IsDeleted && (p.Visibility == PlaylistVisibility.Public || p.UserId == userId)));

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder, 
            defaultSortBy: nameof(UserSavedPlaylist.SavedAt), 
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<UserSavedPlaylist, object>>>
            {
                ["Title"] = usp => usp.Playlist.Title,
                ["SavedAt"] = usp => usp.SavedAt
            });

        var projectedQuery = sortedQuery.Select(usp => new UserFavoritePlaylistDto(
            usp.PlaylistId,
            usp.Playlist.Title,
            usp.Playlist.CoverUrl,
            usp.SavedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
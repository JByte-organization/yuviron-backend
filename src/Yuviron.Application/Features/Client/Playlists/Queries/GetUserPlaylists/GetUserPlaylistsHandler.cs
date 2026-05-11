using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;
using Yuviron.Domain.Enums; 

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserPlaylists;

public sealed class GetUserPlaylistsHandler : IRequestHandler<GetUserPlaylistsQuery, PaginatedList<UserPlaylistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserPlaylistsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<UserPlaylistDto>> Handle(GetUserPlaylistsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var projectedQuery = _context.Playlists
            .AsNoTracking()
            .Where(p => p.UserId == userId && !p.IsDeleted) 
            .OrderByDescending(p => p.UpdatedAt) 
            .Select(p => new UserPlaylistDto(
                p.Id,
                p.Title,
                p.CoverUrl,
                p.Visibility,
                p.PlaylistTracks.Count,
                p.CreatedAt,
                p.UpdatedAt,
                false 
            ));

        var paginatedPlaylists = await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);

        if (request.Page == 1)
        {
            var favoritesCount = await _context.UserSavedTracks
                .CountAsync(ust => ust.UserId == userId, cancellationToken);

            var favoritesPlaylist = new UserPlaylistDto(
                Id: Guid.Empty,
                Title: "Любимые треки", 
                CoverUrl: null, 
                Visibility: PlaylistVisibility.Private,
                TracksCount: favoritesCount,
                CreatedAt: DateTime.MinValue,
                UpdatedAt: DateTime.UtcNow,
                IsSystem: true
            );

            paginatedPlaylists.Items.Insert(0, favoritesPlaylist);
        }

        return paginatedPlaylists;
    }
}
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;

namespace Yuviron.Application.Features.Client.PLaylist.Queries.GetUserPlaylists;

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

        var query = _context.Playlists
            .AsNoTracking()
            .Where(p => p.UserId == userId && !p.IsDeleted);

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder,
            defaultSortBy: nameof(Yuviron.Domain.Entities.Playlist.UpdatedAt), 
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<Domain.Entities.Playlist, object>>>
            {
                [nameof(UserPlaylistDto.TracksCount)] = p => p.PlaylistTracks.Count(pt => !pt.Track.IsDeleted)
            });

        var projectedQuery = sortedQuery.Select(p => new UserPlaylistDto(
            p.Id,
            p.Title,
            p.CoverUrl,
            p.Visibility,
            p.PlaylistTracks.Count(pt => !pt.Track.IsDeleted),
            p.CreatedAt,
            p.UpdatedAt,
            false 
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}

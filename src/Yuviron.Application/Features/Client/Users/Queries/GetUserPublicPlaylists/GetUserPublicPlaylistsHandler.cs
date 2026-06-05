using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching; 
using Yuviron.Application.Abstractions.Services; 
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserPublicPlaylists;

public sealed class GetUserPublicPlaylistsHandler : IRequestHandler<GetUserPublicPlaylistsQuery, PaginatedList<UserPlaylistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache; 
    private readonly ICurrentUserService _currentUser;

    public GetUserPublicPlaylistsHandler(
        IApplicationDbContext context,
        ICacheService cache,
        ICurrentUserService currentUser)
    {
        _context = context;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<UserPlaylistDto>> Handle(GetUserPublicPlaylistsQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.TargetUserId , cancellationToken);
        if (!userExists) throw new NotFoundException(nameof(User), request.TargetUserId);

        var query = _context.Playlists.AsNoTracking().Where(p => p.UserId == request.TargetUserId && p.Visibility == PlaylistVisibility.Public && p.ArtistId == null);

        var sortedQuery = query.ApplySorting(request.SortBy, request.SortOrder, defaultSortBy: nameof(Yuviron.Domain.Entities.Playlist.CreatedAt), defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<Domain.Entities.Playlist, object>>> { [nameof(UserPlaylistDto.TracksCount)] = p => p.PlaylistTracks.Count(pt => !pt.Track.IsDeleted) });

        var projectedQuery = sortedQuery.Select(p => new UserPlaylistDto(
            p.Id,
            p.Title,
            p.CoverUrl,
            p.Visibility,
            p.PlaylistTracks.Count(pt => !pt.Track.IsDeleted),
            p.CreatedAt,
            p.UpdatedAt,
            false, 
            false  
        ));

        var result = await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);

        return await result.EnrichWithCacheAsync(
            _cache, 
            _currentUser.UserId, 
            "saved_playlists", 
            x => x.Id, 
            (x, saved) => x with { IsSaved = saved }, 
            cancellationToken);
    }
}
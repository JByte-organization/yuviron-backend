using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
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

    public GetUserPublicPlaylistsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<UserPlaylistDto>> Handle(GetUserPublicPlaylistsQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == request.TargetUserId , cancellationToken);
        
        if (!userExists)
        {
            throw new NotFoundException(nameof(User), request.TargetUserId);
        }

        var query = _context.Playlists
            .AsNoTracking()
            .Where(p => p.UserId == request.TargetUserId 
                         
                        && p.Visibility == PlaylistVisibility.Public);

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder,
            defaultSortBy: nameof(Yuviron.Domain.Entities.Playlist.CreatedAt), 
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

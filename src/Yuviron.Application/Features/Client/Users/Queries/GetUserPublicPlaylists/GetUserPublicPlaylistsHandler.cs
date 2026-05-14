using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
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
            .AnyAsync(u => u.Id == request.TargetUserId && !u.IsDeleted, cancellationToken);
            
        if (!userExists)
        {
            throw new NotFoundException(nameof(User), request.TargetUserId);
        }

        var projectedQuery = _context.Playlists
            .AsNoTracking()
            .Where(p => p.UserId == request.TargetUserId 
                     && !p.IsDeleted 
                     && p.Visibility == PlaylistVisibility.Public)
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

        var sortedQuery = projectedQuery.ApplySorting(
            request.SortBy, 
            request.SortOrder,
            defaultSortBy: nameof(UserPlaylistDto.CreatedAt), 
            defaultDesc: true);

        return await sortedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
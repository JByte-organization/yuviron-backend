using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;

namespace Yuviron.Application.Features.Client.Playlists.Queries.GetDeletedPlaylists;

public sealed class GetDeletedPlaylistsHandler : IRequestHandler<GetDeletedPlaylistsQuery, PaginatedList<UserPlaylistDto>>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUserService;

    public GetDeletedPlaylistsHandler(ILibraryContext libraryContext, ICurrentUserService currentUserService)
    {
        _libraryContext = libraryContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<UserPlaylistDto>> Handle(GetDeletedPlaylistsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var query = _libraryContext.Playlists
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.IsDeleted)
            .OrderByDescending(p => p.UpdatedAt);

        var projectedQuery = query.Select(p => new UserPlaylistDto(
            p.Id,
            p.Title,
            p.CoverUrl,
            p.Visibility,
            p.PlaylistTracks.Count(), 
            p.CreatedAt,
            p.UpdatedAt,
            false,
            false,
            false
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}

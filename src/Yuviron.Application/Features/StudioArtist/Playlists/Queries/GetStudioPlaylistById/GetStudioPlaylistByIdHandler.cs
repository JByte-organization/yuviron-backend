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
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylistById;

public sealed class GetStudioPlaylistByIdHandler : IRequestHandler<GetStudioPlaylistByIdQuery, StudioPlaylistDetailsDto>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUser;

    public GetStudioPlaylistByIdHandler(ICatalogContext catalogContext, ILibraryContext libraryContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _currentUser = currentUser;
    }

    public async Task<StudioPlaylistDetailsDto> Handle(GetStudioPlaylistByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlistData = await _libraryContext.Playlists
            .AsNoTracking()
            .Where(p => p.Id == request.PlaylistId && !p.IsDeleted)
            .Select(p => new 
            {
                p.ArtistId,
                Dto = new StudioPlaylistDetailsDto(
                    p.Id,
                    p.Title,
                    p.Description,
                    p.CoverUrl,
                    p.Visibility,
                    p.PlaylistTracks.Count(),
                    p.PlaylistTracks.Sum(pt => (long)pt.Track.DurationMs),
                    p.CreatedAt,
                    p.UpdatedAt
                )
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (playlistData is null) throw new NotFoundException(nameof(Playlist), request.PlaylistId);
        if (playlistData.ArtistId == null) throw new ForbiddenException("Not an artist playlist.");

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasViewerAccess(playlistData.ArtistId.Value, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to view this playlist.");

        return playlistData.Dto;
    }
}

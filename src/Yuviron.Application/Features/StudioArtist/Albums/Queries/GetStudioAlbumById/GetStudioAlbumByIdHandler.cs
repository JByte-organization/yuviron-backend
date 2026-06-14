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
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.StudioArtist.Albums.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.GetStudioAlbumById;

public sealed class GetStudioAlbumByIdHandler : IRequestHandler<GetStudioAlbumByIdQuery, StudioAlbumDetailsDto>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;

    public GetStudioAlbumByIdHandler(ICatalogContext catalogContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _currentUser = currentUser;
    }

    public async Task<StudioAlbumDetailsDto> Handle(GetStudioAlbumByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var albumData = await _catalogContext.Albums
            .AsNoTracking()
            .Where(a => a.Id == request.AlbumId && !a.IsDeleted)
            .Select(a => new 
            {
                ArtistIds = a.AlbumArtists.Select(aa => aa.ArtistId).ToList(),
                Dto = new StudioAlbumDetailsDto(
                    a.Id,
                    a.Title,
                    a.Description,
                    a.CoverUrl,
                    a.ReleaseDate,
                    a.ReleaseType,
                    a.VisibilityStatus,
                    a.ScheduledPublishAt,
                    a.Tracks.Count(t => !t.IsDeleted),                           
                    a.Tracks.Where(t => !t.IsDeleted).Sum(t => (long)t.DurationMs),      
                    a.Tracks.Where(t => !t.IsDeleted).Sum(t => t.PlayCount),             
                    a.CreatedAt,
                    a.UpdatedAt,
                    a.AlbumArtists.Select(aa => new TrackArtistDto(aa.ArtistId, aa.Artist.Name, aa.Role))
                )
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (albumData is null) throw new NotFoundException(nameof(Album), request.AlbumId);

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .Where(tm => albumData.ArtistIds.Contains(tm.ArtistId) && tm.UserId == userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("You do not have permission to view this album.");

        return albumData.Dto;
    }
}

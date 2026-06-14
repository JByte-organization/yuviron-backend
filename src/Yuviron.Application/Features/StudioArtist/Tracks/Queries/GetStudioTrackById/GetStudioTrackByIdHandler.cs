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
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTrackById;

public sealed class GetStudioTrackByIdHandler : IRequestHandler<GetStudioTrackByIdQuery, StudioTrackDetailsDto>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;

    public GetStudioTrackByIdHandler(ICatalogContext catalogContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _currentUser = currentUser;
    }

    public async Task<StudioTrackDetailsDto> Handle(GetStudioTrackByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var track = await _catalogContext.Tracks
                        .AsNoTracking()
                        .Include(t => t.TrackArtists).ThenInclude(ta => ta.Artist)
                        .Include(t => t.TrackGenres)
                        .Include(t => t.TrackMoods)
                        .Include(t => t.Lyrics) 
                        .Include(t => t.Album)
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId && !t.IsDeleted, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var artistIds = track.TrackArtists.Select(ta => ta.ArtistId).ToList();
        
        var hasPermission = await _catalogContext.ArtistTeamMembers
            .Where(tm => artistIds.Contains(tm.ArtistId) && tm.UserId == userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to this track.");

        return new StudioTrackDetailsDto(
            track.Id,
            track.AlbumId,
            track.Album?.Title ?? "Unknown Album",
            track.AlbumPosition,
            track.Title,
            track.DurationMs,
            track.Explicit,
            track.CoverUrl ?? track.Album?.CoverUrl,
            track.ProcessingStatus,
            track.VisibilityStatus,
            track.PlayCount,
            track.CreatedAt,
            track.UpdatedAt,
            track.Lyrics?.PlainText, 
            track.TrackArtists.Select(ta => new TrackArtistDto(ta.ArtistId, ta.Artist.Name, ta.Role)),
            track.TrackGenres.Select(tg => tg.GenreId),
            track.TrackMoods.Select(tm => tm.MoodId)
        );
    }
}

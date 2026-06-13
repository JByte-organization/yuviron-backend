using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.StudioArtist.Albums.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.GetStudioAlbumTracks;

public sealed class GetStudioAlbumTracksHandler : IRequestHandler<GetStudioAlbumTracksQuery, List<StudioAlbumTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetStudioAlbumTracksHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<StudioAlbumTrackDto>> Handle(GetStudioAlbumTracksQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var albumInfo = await _context.Albums
            .AsNoTracking()
            .Where(a => a.Id == request.AlbumId)
            .Select(a => new { ArtistIds = a.AlbumArtists.Select(aa => aa.ArtistId).ToList() })
            .FirstOrDefaultAsync(cancellationToken);

        if (albumInfo == null) throw new NotFoundException(nameof(Album), request.AlbumId);

        var hasPermission = await _context.ArtistTeamMembers
            .Where(tm => albumInfo.ArtistIds.Contains(tm.ArtistId) && tm.UserId == userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to this album.");

        return await _context.Tracks
            .AsNoTracking()
            .Where(t => t.AlbumId == request.AlbumId)
            .OrderBy(t => t.AlbumPosition)
            .Select(t => new StudioAlbumTrackDto(
                t.Id,
                t.AlbumPosition,
                t.Title,
                t.DurationMs,
                t.Explicit,
                t.CoverUrl,
                t.ProcessingStatus,
                t.VisibilityStatus
            ))
            .ToListAsync(cancellationToken);
    }
}

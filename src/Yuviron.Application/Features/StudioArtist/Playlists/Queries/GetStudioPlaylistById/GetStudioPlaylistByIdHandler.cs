using MediatR;
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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetStudioPlaylistByIdHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<StudioPlaylistDetailsDto> Handle(GetStudioPlaylistByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlistData = await _context.Playlists
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

        var hasPermission = await _context.ArtistTeamMembers
            .HasManagementAccess(playlistData.ArtistId.Value, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to view this playlist.");

        return playlistData.Dto;
    }
}
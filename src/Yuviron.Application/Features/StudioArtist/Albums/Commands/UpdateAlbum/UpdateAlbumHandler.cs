using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Albums.Commands.UpdateAlbum;

public sealed class UpdateAlbumHandler : IRequestHandler<UpdateAlbumCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdateAlbumHandler(
        IApplicationDbContext context,
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateAlbumCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var album = await _context.Albums
            .Include(a => a.AlbumArtists)
            .FirstOrDefaultAsync(a => a.Id == request.AlbumId, cancellationToken);

        if (album == null)
        {
            throw new NotFoundException(nameof(Album), request.AlbumId);
        }

        var albumArtistIds = album.AlbumArtists.Select(aa => aa.ArtistId).ToList();
        
        var hasPermission = await _context.ArtistTeamMembers
            .HasManagementAccess(albumArtistIds, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission)
        {
            throw new ForbiddenException("You do not have permission to modify this album.");
        }

        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _context.ClaimFileAsync(
                request.CoverFileId.Value, 
                userId, 
                "image/", 
                "covers", 
                cancellationToken);
        }

        var finalCoverUrl = coverClaim != null ? coverClaim.FinalPath : album.CoverUrl;

        album.UpdateDetails(
            request.Title,
            request.Description,
            finalCoverUrl,
            request.ReleaseDate,
            request.ReleaseType,
            album.VisibilityStatus, 
            album.ScheduledPublishAt,
            albumArtistIds,
            utcNow
        );

        if (coverClaim != null)
        {
            album.RegisterFileSwapEvents(coverClaim);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
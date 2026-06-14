using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteLyrics;

public sealed class DeleteLyricsHandler : IRequestHandler<DeleteLyricsCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;

    public DeleteLyricsHandler(ICatalogContext catalogContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteLyricsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var track = await _catalogContext.Tracks
                        .Include(t => t.Lyrics) 
                        .Include(t => t.Album).ThenInclude(a => a!.AlbumArtists)
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId && !t.IsDeleted, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .HasManagementAccess(track.Album!.AlbumArtists.Select(aa => aa.ArtistId), userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this track.");

        track.SetLyrics(null);

        await _catalogContext.SaveChangesAsync(cancellationToken);
    
        return Unit.Value;
    }
}
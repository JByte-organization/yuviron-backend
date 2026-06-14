using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteTrack;

public sealed class DeleteTrackHandler : IRequestHandler<DeleteTrackCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public DeleteTrackHandler(ICatalogContext catalogContext, TimeProvider timeProvider, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext; _timeProvider = timeProvider; _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteTrackCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var track = await _catalogContext.Tracks.Include(t => t.Album).ThenInclude(a => a!.AlbumArtists)
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        // Access only for Main artists of the album/track
        var mainArtistIds = track.Album!.AlbumArtists
            .Where(aa => aa.Role == ArtistRole.Main)
            .Select(aa => aa.ArtistId)
            .ToList();

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .HasManagementAccess(mainArtistIds, userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this track (only Main artists or managers).");

        track.Delete(_timeProvider.GetUtcNow().UtcDateTime); 
        await _catalogContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

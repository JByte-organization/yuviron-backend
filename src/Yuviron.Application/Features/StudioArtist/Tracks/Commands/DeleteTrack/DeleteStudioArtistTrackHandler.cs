using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteTrack;

public sealed class DeleteStudioArtistTrackHandler : IRequestHandler<DeleteStudioArtistTrackCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public DeleteStudioArtistTrackHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteStudioArtistTrackCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var currentArtistId = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.UserId == userId)
            .OrderBy(tm => tm.Role == ArtistTeamRole.Owner ? 0 :
                tm.Role == ArtistTeamRole.Manager ? 1 :
                tm.Role == ArtistTeamRole.Editor ? 2 :
                tm.Role == ArtistTeamRole.Viewer ? 3 : 4)
            .ThenByDescending(tm => tm.CreatedAt)
            .Select(tm => (Guid?)tm.ArtistId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!currentArtistId.HasValue)
        {
            throw new NotFoundException(nameof(Artist), $"for user {userId}");
        }

        var track = await _context.Tracks
            .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
            ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var belongsToCurrentArtist = await _context.AlbumArtists
            .AsNoTracking()
            .AnyAsync(
                aa => aa.AlbumId == track.AlbumId && aa.ArtistId == currentArtistId.Value,
                cancellationToken);

        if (!belongsToCurrentArtist)
        {
            throw new ForbiddenException("You can only delete tracks linked to albums of your current artist profile.");
        }

        track.Delete(_timeProvider.GetUtcNow().UtcDateTime);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

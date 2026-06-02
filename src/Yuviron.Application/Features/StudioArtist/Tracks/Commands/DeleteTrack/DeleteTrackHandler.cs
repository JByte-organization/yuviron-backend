using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteTrack;

public sealed class DeleteTrackHandler : IRequestHandler<DeleteTrackCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public DeleteTrackHandler(IApplicationDbContext context, TimeProvider timeProvider, ICurrentUserService currentUser)
    {
        _context = context; _timeProvider = timeProvider; _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteTrackCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var track = await _context.Tracks.Include(t => t.Album).ThenInclude(a => a!.AlbumArtists)
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var hasAccess = await _context.ArtistTeamMembers
            .HasManagementAccess(track.Album!.AlbumArtists.Select(aa => aa.ArtistId), userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this track.");

        track.Delete(_timeProvider.GetUtcNow().UtcDateTime); 
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
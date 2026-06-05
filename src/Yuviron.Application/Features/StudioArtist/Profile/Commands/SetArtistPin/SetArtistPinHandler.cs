using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.SetArtistPin;

public sealed class SetArtistPinHandler : IRequestHandler<SetArtistPinCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public SetArtistPinHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _context = context; _currentUser = currentUser; _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(SetArtistPinCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var artist = await _context.Artists
            .Include(a => a.TeamMembers)
            .Include(a => a.Pins)
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var hasAccess = artist.TeamMembers.Any(tm => tm.UserId == userId);
        if (!hasAccess) throw new ForbiddenException("No access to manage this artist's profile.");

        bool entityExists = request.EntityType switch
        {
            ArtistPinType.Track => await _context.Tracks.AnyAsync(t => t.Id == request.EntityId, cancellationToken),
            ArtistPinType.Album => await _context.Albums.AnyAsync(a => a.Id == request.EntityId, cancellationToken),
            ArtistPinType.Playlist => await _context.Playlists.AnyAsync(p => p.Id == request.EntityId, cancellationToken),
            _ => false
        };

        if (!entityExists)
        {
            throw new NotFoundException(request.EntityType.ToString(), request.EntityId);
        }

        artist.SetPin(request.EntityType, request.EntityId, request.Position, utcNow);

        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
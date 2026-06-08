using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateLyrics;

public sealed class UpdateLyricsHandler : IRequestHandler<UpdateLyricsCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IEventBus _eventBus;

    public UpdateLyricsHandler(IApplicationDbContext context, ICurrentUserService currentUser, IEventBus eventBus)
    {
        _context = context; 
        _currentUser = currentUser;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(UpdateLyricsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var track = await _context.Tracks
                        .Include(t => t.TrackArtists)
                        .Include(t => t.Lyrics) 
                        .Include(t => t.Album).ThenInclude(a => a!.AlbumArtists)
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId && !t.IsDeleted, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var hasAccess = await _context.ArtistTeamMembers
            .HasManagementAccess(track.Album!.AlbumArtists.Select(aa => aa.ArtistId), userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this track.");

        var artistId = track.TrackArtists.FirstOrDefault(ta => ta.Role == ArtistRole.Main)?.ArtistId
                       ?? track.Album!.AlbumArtists.FirstOrDefault(aa => aa.Role == ArtistRole.Main)?.ArtistId
                       ?? track.Album!.AlbumArtists.First().ArtistId;

        track.SetLyrics(request.LyricsText);

        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new TrackLyricsUpdatedEvent(artistId, track.Id, track.Title, userId),
            cancellationToken);
    
        return Unit.Value;
    }
}

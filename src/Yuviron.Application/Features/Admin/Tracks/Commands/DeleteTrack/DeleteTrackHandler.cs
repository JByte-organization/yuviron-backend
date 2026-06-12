using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.DeleteTrack;

public sealed class DeleteTrackHandler : IRequestHandler<DeleteTrackCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public DeleteTrackHandler(IApplicationDbContext context, TimeProvider timeProvider, IEventBus eventBus) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(DeleteTrackCommand request, CancellationToken cancellationToken)
    {
        var track = await _context.Tracks
                        .Include(t => t.TrackArtists)
                        .Include(t => t.Album).ThenInclude(a => a!.AlbumArtists)
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var artistId = track.TrackArtists.FirstOrDefault(ta => ta.Role == ArtistRole.Main)?.ArtistId
                       ?? track.Album?.AlbumArtists.FirstOrDefault(aa => aa.Role == ArtistRole.Main)?.ArtistId
                       ?? track.Album?.AlbumArtists.FirstOrDefault()?.ArtistId ?? Guid.Empty;

        track.Delete(_timeProvider.GetUtcNow().UtcDateTime); 

        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new ModeratedTrackDeletedEvent(track.Id, artistId, track.Title),
            cancellationToken);

        return Unit.Value;
    }
}


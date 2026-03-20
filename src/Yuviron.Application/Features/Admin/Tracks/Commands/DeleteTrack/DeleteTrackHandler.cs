using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.DeleteTrack;

public sealed class DeleteTrackHandler : IRequestHandler<DeleteTrackCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public DeleteTrackHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider) 
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteTrackCommand request, CancellationToken cancellationToken)
    {
        var track = await _context.Tracks
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var coverUrlToDelete = track.CoverUrl;
        var audioKeyToDelete = track.AudioStorageKey;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        track.Delete(utcNow); 

        if (!string.IsNullOrWhiteSpace(coverUrlToDelete))
            track.AddDomainEvent(new FileNeedsDeletionEvent(coverUrlToDelete));

        if (!string.IsNullOrWhiteSpace(audioKeyToDelete))
            track.AddDomainEvent(new FileNeedsDeletionEvent(audioKeyToDelete));

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
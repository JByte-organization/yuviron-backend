using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Moods.Commands.DeleteMood;

public sealed class DeleteMoodHandler : IRequestHandler<DeleteMoodCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public DeleteMoodHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteMoodCommand request, CancellationToken cancellationToken)
    {
        var mood = await _context.Moods
                       .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
                   ?? throw new NotFoundException(nameof(Mood), request.Id);

        var hasAssociatedTracks = await _context.TrackMoods
            .AnyAsync(tm => tm.MoodId == request.Id && !tm.Track.IsDeleted, cancellationToken);

        if (hasAssociatedTracks)
        {
            throw new InvalidOperationException("Cannot delete this mood because it is currently associated with one or more tracks.");
        }

        mood.Delete(_timeProvider.GetUtcNow().UtcDateTime);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

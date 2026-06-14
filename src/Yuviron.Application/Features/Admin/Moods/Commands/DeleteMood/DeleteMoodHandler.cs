using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Moods.Commands.DeleteMood;

public sealed class DeleteMoodHandler : IRequestHandler<DeleteMoodCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;

    public DeleteMoodHandler(ICatalogContext catalogContext, TimeProvider timeProvider)
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteMoodCommand request, CancellationToken cancellationToken)
    {
        var mood = await _catalogContext.Moods
                       .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
                   ?? throw new NotFoundException(nameof(Mood), request.Id);

        var hasAssociatedTracks = await _catalogContext.TrackMoods
            .AnyAsync(tm => tm.MoodId == request.Id && !tm.Track.IsDeleted, cancellationToken);

        if (hasAssociatedTracks)
        {
            throw new InvalidOperationException("Cannot delete this mood because it is currently associated with one or more tracks.");
        }

        mood.Delete(_timeProvider.GetUtcNow().UtcDateTime);

        await _catalogContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Moods.Commands.UpdateMood;

public sealed class UpdateMoodHandler : IRequestHandler<UpdateMoodCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;


    public UpdateMoodHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateMoodCommand request, CancellationToken cancellationToken)
    {
        var mood = await _context.Moods
                       .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
                   ?? throw new NotFoundException(nameof(Mood), request.Id);

        if (!mood.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Moods.AnyAsync(m => m.Name == request.Name, cancellationToken))
                throw new InvalidOperationException($"Mood with name '{request.Name}' already exists.");
        }

        var oldCoverUrl = mood.CoverUrl;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var finalCoverUrl = FileStorageExtensions.PredictDestinationPath(request.CoverUrl, "covers");

        mood.Update(request.Name, finalCoverUrl, utcNow); 

        if (!string.IsNullOrWhiteSpace(request.CoverUrl) && request.CoverUrl.StartsWith("temp/"))
        {
            mood.AddDomainEvent(new TempFileNeedsMovingEvent(request.CoverUrl, "covers"));
        }

        if (!string.Equals(oldCoverUrl, finalCoverUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldCoverUrl))
        {
            mood.AddDomainEvent(new FileNeedsDeletionEvent(oldCoverUrl));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
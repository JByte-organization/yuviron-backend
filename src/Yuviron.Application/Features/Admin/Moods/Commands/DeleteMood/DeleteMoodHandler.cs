using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Moods.Commands.DeleteMood;

public sealed class DeleteMoodHandler : IRequestHandler<DeleteMoodCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService;

    public DeleteMoodHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(DeleteMoodCommand request, CancellationToken cancellationToken)
    {
        var mood = await _context.Moods
                       .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
                   ?? throw new NotFoundException(nameof(Mood), request.Id);

        var hasAssociatedTracks = await _context.TrackMoods
            .AnyAsync(tm => tm.MoodId == request.Id, cancellationToken);

        if (hasAssociatedTracks)
        {
            throw new InvalidOperationException("Cannot delete this mood because it is currently associated with one or more tracks.");
        }

        var coverUrlToDelete = mood.CoverUrl;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        mood.Delete(utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(coverUrlToDelete))
        {
            await _fileStorageService.DeleteAsync(coverUrlToDelete, cancellationToken);
        }

        return Unit.Value;
    }
}
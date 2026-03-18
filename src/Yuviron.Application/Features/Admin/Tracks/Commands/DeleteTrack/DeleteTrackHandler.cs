using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.DeleteTrack;

public sealed class DeleteTrackHandler : IRequestHandler<DeleteTrackCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService; 

    public DeleteTrackHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
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

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(coverUrlToDelete))
            await _fileStorageService.DeleteAsync(coverUrlToDelete, cancellationToken);
            
        if (!string.IsNullOrWhiteSpace(audioKeyToDelete))
            await _fileStorageService.DeleteAsync(audioKeyToDelete, cancellationToken);
            

        return Unit.Value;
    }
}
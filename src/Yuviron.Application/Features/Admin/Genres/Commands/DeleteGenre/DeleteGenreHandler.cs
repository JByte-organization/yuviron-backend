using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services; // <-- Добавили
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions; 

namespace Yuviron.Application.Features.Admin.Genres.Commands.DeleteGenre;

public sealed class DeleteGenreHandler : IRequestHandler<DeleteGenreCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService; // <-- Добавили

    public DeleteGenreHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(DeleteGenreCommand request, CancellationToken cancellationToken)
    {
        var genre = await _context.Genres
                        .FirstOrDefaultAsync(g => g.Id == request.GenreId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Genre), request.GenreId);

        var hasAssociatedTracks = await _context.TrackGenres
            .AnyAsync(tg => tg.GenreId == request.GenreId, cancellationToken);

        if (hasAssociatedTracks)
        {
            throw new InvalidOperationException("Cannot delete this genre because it is currently associated with one or more tracks.");
        }

        var coverUrlToDelete = genre.CoverUrl; 
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        genre.Delete(utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(coverUrlToDelete))
        {
            await _fileStorageService.DeleteAsync(coverUrlToDelete, cancellationToken);
        }

        return Unit.Value;
    }
}
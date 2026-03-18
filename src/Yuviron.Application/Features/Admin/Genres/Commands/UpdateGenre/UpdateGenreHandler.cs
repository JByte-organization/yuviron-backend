using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; // <-- Добавили
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Genres.Commands.UpdateGenre;

public sealed class UpdateGenreHandler : IRequestHandler<UpdateGenreCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService; // <-- Добавили

    public UpdateGenreHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(UpdateGenreCommand request, CancellationToken cancellationToken)
    {
        var genre = await _context.Genres
                        .FirstOrDefaultAsync(g => g.Id == request.GenreId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Genre), request.GenreId);

        if (!genre.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Genres.AnyAsync(g => g.Name == request.Name, cancellationToken))
                throw new InvalidOperationException($"Genre '{request.Name}' already exists.");
        }

        var oldCoverUrl = genre.CoverUrl; 
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var finalCoverUrl = await _fileStorageService.MoveIfTempAsync(request.CoverUrl, "covers", cancellationToken);
        
        genre.Update(request.Name, finalCoverUrl, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.Equals(oldCoverUrl, finalCoverUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldCoverUrl))
        {
            await _fileStorageService.DeleteAsync(oldCoverUrl, cancellationToken);
        }

        return Unit.Value;
    }
}
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Genres.Commands.UpdateGenre;

public sealed class UpdateGenreHandler : IRequestHandler<UpdateGenreCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateGenreHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
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
        
        var finalCoverUrl = FileStorageExtensions.PredictDestinationPath(request.CoverUrl, "covers");
        
        genre.Update(request.Name, finalCoverUrl, utcNow);

        if (!string.IsNullOrWhiteSpace(request.CoverUrl) && request.CoverUrl.StartsWith("temp/"))
        {
            genre.AddDomainEvent(new TempFileNeedsMovingEvent(request.CoverUrl, "covers"));
        }

        if (!string.Equals(oldCoverUrl, finalCoverUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldCoverUrl))
        {
            genre.AddDomainEvent(new FileNeedsDeletionEvent(oldCoverUrl));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
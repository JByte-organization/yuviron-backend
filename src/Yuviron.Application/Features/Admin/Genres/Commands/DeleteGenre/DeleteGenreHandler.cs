using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions; 

namespace Yuviron.Application.Features.Admin.Genres.Commands.DeleteGenre;

public sealed class DeleteGenreHandler : IRequestHandler<DeleteGenreCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public DeleteGenreHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteGenreCommand request, CancellationToken cancellationToken)
    {
        var genre = await _context.Genres
                        .FirstOrDefaultAsync(g => g.Id == request.GenreId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Genre), request.GenreId);

        var hasAssociatedTracks = await _context.TrackGenres
            .AnyAsync(tg => tg.GenreId == request.GenreId && !tg.Track.IsDeleted, cancellationToken);

        if (hasAssociatedTracks)
        {
            throw new InvalidOperationException("Cannot delete this genre because it is currently associated with one or more tracks.");
        }

        genre.Delete(_timeProvider.GetUtcNow().UtcDateTime);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

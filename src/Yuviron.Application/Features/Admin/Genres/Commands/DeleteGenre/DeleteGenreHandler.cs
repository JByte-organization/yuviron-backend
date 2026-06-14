using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions; 

namespace Yuviron.Application.Features.Admin.Genres.Commands.DeleteGenre;

public sealed class DeleteGenreHandler : IRequestHandler<DeleteGenreCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;

    public DeleteGenreHandler(ICatalogContext catalogContext, TimeProvider timeProvider)
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteGenreCommand request, CancellationToken cancellationToken)
    {
        var genre = await _catalogContext.Genres
                        .FirstOrDefaultAsync(g => g.Id == request.GenreId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Genre), request.GenreId);

        var hasAssociatedTracks = await _catalogContext.TrackGenres
            .AnyAsync(tg => tg.GenreId == request.GenreId, cancellationToken);

        if (hasAssociatedTracks)
        {
            throw new InvalidOperationException("Cannot delete this genre because it is currently associated with one or more tracks.");
        }

        genre.Delete(_timeProvider.GetUtcNow().UtcDateTime);

        await _catalogContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
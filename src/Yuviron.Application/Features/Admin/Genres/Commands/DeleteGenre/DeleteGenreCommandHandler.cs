using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Admin.Genres.Commands.DeleteGenre;

public sealed class DeleteGenreCommandHandler : IRequestHandler<DeleteGenreCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public DeleteGenreCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteGenreCommand request, CancellationToken cancellationToken)
    {
        var genre = await _context.Genres
            .FirstOrDefaultAsync(g => g.Id == request.GenreId, cancellationToken);

        if (genre == null)
            throw new Exception($"Genre with ID {request.GenreId} not found.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        // Soft Delete!
        genre.Delete(utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Genres.Commands.UpdateGenre;

public sealed class UpdateGenreHandler : IRequestHandler<UpdateGenreCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateGenreHandler(IApplicationDbContext context, TimeProvider timeProvider)
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

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        genre.Update(request.Name, request.CoverUrl, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
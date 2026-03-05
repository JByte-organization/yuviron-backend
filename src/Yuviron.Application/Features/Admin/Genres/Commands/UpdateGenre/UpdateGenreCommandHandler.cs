using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Admin.Genres.Commands.UpdateGenre;

public sealed class UpdateGenreCommandHandler : IRequestHandler<UpdateGenreCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateGenreCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateGenreCommand request, CancellationToken cancellationToken)
    {
        var genre = await _context.Genres
            .FirstOrDefaultAsync(g => g.Id == request.GenreId, cancellationToken);

        if (genre == null)
            throw new Exception($"Genre with ID {request.GenreId} not found.");

        // Проверяем уникальность имени, ТОЛЬКО если админ решил его изменить
        if (!genre.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Genres.AnyAsync(g => g.Name == request.Name, cancellationToken))
                throw new InvalidOperationException($"Genre '{request.Name}' already exists.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        // Вызываем наш инкапсулированный метод домена
        genre.Update(request.Name, request.CoverUrl, request.HexColor, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
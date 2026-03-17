using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Genres.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Genres.Queries.GetGenresById;

public sealed class GetGenreByIdHandler : IRequestHandler<GetGenreByIdQuery, GenreDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetGenreByIdHandler(IApplicationDbContext context) => _context = context;

    public async Task<GenreDetailsDto> Handle(GetGenreByIdQuery request, CancellationToken cancellationToken)
    {
        var genre = await _context.Genres
            .AsNoTracking()
            .Where(g => g.Id == request.GenreId)
            .Select(g => new GenreDetailsDto(
                g.Id,
                g.Name,
                g.CoverUrl != null ? $"/storage/{g.CoverUrl}" : null
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (genre is null)
            throw new NotFoundException(nameof(Genre), request.GenreId);

        return genre;
    }
}
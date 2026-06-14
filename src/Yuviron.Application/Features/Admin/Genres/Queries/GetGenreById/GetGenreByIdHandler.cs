using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Genres.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Genres.Queries.GetGenresById;

public sealed class GetGenreByIdHandler : IRequestHandler<GetGenreByIdQuery, GenreDetailsDto>
{
    private readonly ICatalogContext _catalogContext;

    public GetGenreByIdHandler(ICatalogContext catalogContext) => _catalogContext = catalogContext;

    public async Task<GenreDetailsDto> Handle(GetGenreByIdQuery request, CancellationToken cancellationToken)
    {
        var genre = await _catalogContext.Genres
            .AsNoTracking()
            .Where(g => g.Id == request.GenreId )
            .Select(g => new GenreDetailsDto(
                g.Id,
                g.Name,
                g.CoverUrl,
                g.TrackGenres.Count(tg => !tg.Track.IsDeleted),
                g.CreatedAt,
                g.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (genre is null)
            throw new NotFoundException(nameof(Genre), request.GenreId);

        return genre;
    }
}

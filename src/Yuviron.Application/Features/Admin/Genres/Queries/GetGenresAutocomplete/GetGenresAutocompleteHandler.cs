using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Admin.Genres.Queries.GetGenresAutocomplete;

public sealed class GetGenresAutocompleteHandler : IRequestHandler<GetGenresAutocompleteQuery, List<GenreAutocompleteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGenresAutocompleteHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GenreAutocompleteDto>> Handle(GetGenresAutocompleteQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return new List<GenreAutocompleteDto>();

        var searchTerm = request.SearchTerm.Trim().ToLower();

        return await _context.Genres
            .AsNoTracking()
            .Where(g => !g.IsDeleted && g.Name.ToLower().Contains(searchTerm))
            .OrderBy(g => g.Name)
            .Take(request.Limit)
            .Select(g => new GenreAutocompleteDto(
                g.Id,
                g.Name,
                g.CoverUrl
            ))
            .ToListAsync(cancellationToken);
    }
}
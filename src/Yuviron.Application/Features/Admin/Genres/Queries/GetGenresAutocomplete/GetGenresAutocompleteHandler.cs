using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
    private readonly ICatalogContext _catalogContext;

    public GetGenresAutocompleteHandler(ICatalogContext catalogContext)
    {
        _catalogContext = catalogContext;
    }

    public async Task<List<GenreAutocompleteDto>> Handle(GetGenresAutocompleteQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return new List<GenreAutocompleteDto>();

        var searchTerm = request.SearchTerm.Trim();

        return await _catalogContext.Genres
            .AsNoTracking()
            .Where(g => !g.IsDeleted && g.Name.Contains(searchTerm)) 
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions; // Здесь ToPaginatedListAsync
using Yuviron.Application.Features.Admin.Genres.Queries.DTOs;

namespace Yuviron.Application.Features.Admin.Genres.Queries.GetGenres;

public sealed class GetGenresHandler : IRequestHandler<GetGenresQuery, PaginatedList<GenreDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGenresHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<GenreDto>> Handle(GetGenresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Genres.AsNoTracking();

        if (request.IncludeDeleted) 
        {
            query = query.IgnoreQueryFilters();
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(g => g.Name.StartsWith(request.SearchTerm));
        }

        var projectedQuery = query
            .OrderBy(g => g.Name) 
            .Select(g => new GenreDto(
                g.Id, 
                g.Name, 
                g.CoverUrl,
                g.IsDeleted, 
                g.CreatedAt
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs; // PaginatedList
using Yuviron.Application.Features.Admin.Genres.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Genres.Queries.GetGenres;

public sealed record GetGenresQuery(
    string? SearchTerm,
    bool IncludeDeleted,
    int Page = 1,
    int PageSize = 50 // Жанров обычно немного, отдаем пачкой побольше
) : IRequest<PaginatedList<GenreDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}

public sealed class GetGenresQueryHandler : IRequestHandler<GetGenresQuery, PaginatedList<GenreDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGenresQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<GenreDto>> Handle(GetGenresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Genres.AsNoTracking();

        if (request.IncludeDeleted) query = query.IgnoreQueryFilters();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(g => g.Name.Contains(request.SearchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(g => g.Name) // Жанры лучше сортировать по алфавиту
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(g => new GenreDto(g.Id, g.Name, g.CoverUrl, g.HexColor, g.IsDeleted, g.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedList<GenreDto>(items, totalCount, request.Page, request.PageSize);
    }
}
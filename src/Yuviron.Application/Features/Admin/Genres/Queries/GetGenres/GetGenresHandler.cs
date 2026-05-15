using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions; 
using Yuviron.Application.Features.Admin.Genres.Queries.DTOs;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Genres.Queries.GetGenres;

public sealed class GetGenresHandler : IRequestHandler<GetGenresQuery, PaginatedList<GenreListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGenresHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<GenreListItemDto>> Handle(GetGenresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Genres
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(g => g.Name.StartsWith(request.SearchTerm));

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder,
            defaultSortBy: "CreatedAt",
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<Genre, object>>>
            {
                [nameof(GenreListItemDto.TracksCount)] = g => g.TrackGenres.Count(tg => !tg.Track.IsDeleted)
            });

        var projectedQuery = sortedQuery.Select(g => new GenreListItemDto(
            g.Id, 
            g.CoverUrl,
            g.Name, 
            g.TrackGenres.Count(tg => !tg.Track.IsDeleted),
            g.CreatedAt,
            g.UpdatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}

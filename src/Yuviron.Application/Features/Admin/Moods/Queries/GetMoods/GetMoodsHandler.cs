using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Admin.Moods.Queries.GetMoods;

public sealed class GetMoodsHandler : IRequestHandler<GetMoodsQuery, PaginatedList<MoodDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMoodsHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<MoodDto>> Handle(GetMoodsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Moods
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(m => m.Name.StartsWith(request.SearchTerm));
        }

        var projectedQuery = query
            .Select(m => new MoodDto(
                m.Id, 
                m.Name,
                m.CoverUrl,
                m.TrackMoods.Count, 
                m.CreatedAt,
                m.UpdatedAt
            ));

        var sortedQuery = projectedQuery.ApplySorting(request.SortBy, request.SortOrder);

        return await sortedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
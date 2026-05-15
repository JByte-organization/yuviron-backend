using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

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

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder,
            defaultSortBy: "CreatedAt",
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<Mood, object>>>
            {
                [nameof(MoodDto.TracksCount)] = m => m.TrackMoods.Count(tm => !tm.Track.IsDeleted)
            });

        var projectedQuery = sortedQuery.Select(m => new MoodDto(
            m.Id, 
            m.Name,
            m.CoverUrl,
            m.TrackMoods.Count(tm => !tm.Track.IsDeleted), 
            m.CreatedAt,
            m.UpdatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}

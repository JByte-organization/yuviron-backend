using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        var query = _context.Moods.AsNoTracking();

        if (request.IncludeDeleted) query = query.IgnoreQueryFilters();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(m => m.Name.Contains(request.SearchTerm));
        }

        var projectedQuery = query
            .OrderBy(m => m.Name) 
            .Select(m => new MoodDto(
                m.Id, 
                m.Name, 
                m.CoverUrl, 
                m.IsDeleted, 
                m.CreatedAt
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Client.Moods.Queries.GetMoods;

public sealed class GetMoodsHandler : IRequestHandler<GetMoodsQuery, List<MoodItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMoodsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MoodItemDto>> Handle(GetMoodsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Moods
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.Name) 
            .Take(request.Limit)
            .Select(m => new MoodItemDto(
                m.Id,
                m.Name,
                m.CoverUrl
            ))
            .ToListAsync(cancellationToken);
    }
}
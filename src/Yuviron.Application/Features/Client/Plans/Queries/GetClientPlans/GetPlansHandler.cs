using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Client.Plans.Queries.GetClientPlans;

public sealed class GetPlansHandler : IRequestHandler<GetPlansQuery, List<PlanDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPlansHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<PlanDto>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Plans.AsNoTracking().Where(p => p.Price > 0);

        if (request.Type.HasValue)
        {
            query = query.Where(p => p.Type == request.Type.Value);
        }

        return await query
            .OrderBy(p => p.Price)
            .Select(p => new PlanDto(
                p.Id, 
                p.Name, 
                p.Price, 
                p.Currency, 
                p.Period,
                p.Type 
            ))
            .ToListAsync(cancellationToken);
    }
}
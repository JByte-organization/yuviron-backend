using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Plans.Queries.GetPlanById;

public sealed class GetPlanByIdHandler : IRequestHandler<GetPlanByIdQuery, PlanDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetPlanByIdHandler(IApplicationDbContext context) => _context = context;

    public async Task<PlanDetailsDto> Handle(GetPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PlanDetailsDto(
                p.Id,
                p.Name,
                p.Price,
                p.Currency,
                p.Period,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (plan is null) throw new NotFoundException(nameof(Plan), request.Id);

        return plan;
    }
}
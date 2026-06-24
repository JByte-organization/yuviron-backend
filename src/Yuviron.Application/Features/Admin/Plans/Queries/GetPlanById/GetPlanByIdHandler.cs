using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Plans.Queries.GetPlanById;

public sealed class GetPlanByIdHandler : IRequestHandler<GetPlanByIdQuery, PlanDetailsDto>
{
    private readonly IMonetizationContext _monetizationContext;

    public GetPlanByIdHandler(IMonetizationContext monetizationContext) => _monetizationContext = monetizationContext;

    public async Task<PlanDetailsDto> Handle(GetPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await _monetizationContext.Plans
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PlanDetailsDto(
                p.Id,
                p.Name,
                p.Price,
                p.Currency,
                p.Period,
                p.Type == PlanType.Listener ? nameof(PlanType.Listener) :
                p.Type == PlanType.Artist ? nameof(PlanType.Artist) :
                nameof(PlanType.Unknown),
                p.CreatedAt,
                p.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (plan is null) throw new NotFoundException(nameof(Plan), request.Id);

        return plan;
    }
}

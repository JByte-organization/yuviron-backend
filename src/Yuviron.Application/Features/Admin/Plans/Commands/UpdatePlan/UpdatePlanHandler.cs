using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Plans.Commands.UpdatePlan;

public sealed class UpdatePlanHandler : IRequestHandler<UpdatePlanCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdatePlanHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (plan == null) throw new NotFoundException(nameof(Plan), request.Id);

        if (!plan.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Plans.AnyAsync(p => p.Name == request.Name, cancellationToken))
            {
                throw new InvalidOperationException($"Plan with name '{request.Name}' already exists.");
            }
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        plan.Update(request.Name, request.Price, request.Currency, request.Period, request.Type, utcNow); 
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
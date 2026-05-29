using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Plans.Commands.DeletePlan;

public sealed class DeletePlanHandler : IRequestHandler<DeletePlanCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeletePlanHandler(IApplicationDbContext context) => _context = context;

    public async Task<Unit> Handle(DeletePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans
                       .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
                   ?? throw new NotFoundException(nameof(Plan), request.Id);

        var hasActiveSubscriptions = await _context.Subscriptions
            .AnyAsync(s => s.PlanId == request.Id && s.Status == SubscriptionStatus.Active, cancellationToken);

        if (hasActiveSubscriptions)
        {
            throw new InvalidOperationException("Cannot delete this plan because it has active subscriptions.");
        }

        _context.Plans.Remove(plan);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
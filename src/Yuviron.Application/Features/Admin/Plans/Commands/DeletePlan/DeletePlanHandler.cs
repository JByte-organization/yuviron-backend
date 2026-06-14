using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
    private readonly IMonetizationContext _monetizationContext;

    public DeletePlanHandler(IMonetizationContext monetizationContext) => _monetizationContext = monetizationContext;

    public async Task<Unit> Handle(DeletePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _monetizationContext.Plans
                       .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
                   ?? throw new NotFoundException(nameof(Plan), request.Id);

        var hasActiveSubscriptions = await _monetizationContext.Subscriptions
            .AnyAsync(s => s.PlanId == request.Id && s.Status == SubscriptionStatus.Active, cancellationToken);

        if (hasActiveSubscriptions)
        {
            throw new InvalidOperationException("Cannot delete this plan because it has active subscriptions.");
        }

        _monetizationContext.Remove(plan);
        await _monetizationContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
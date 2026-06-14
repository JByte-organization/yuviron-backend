using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Plans.Commands.CreatePlan;

public sealed class CreatePlanHandler : IRequestHandler<CreatePlanCommand, Guid>
{
    private readonly IMonetizationContext _monetizationContext;
    private readonly TimeProvider _timeProvider;

    public CreatePlanHandler(IMonetizationContext monetizationContext, TimeProvider timeProvider)
    {
        _monetizationContext = monetizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        if (await _monetizationContext.Plans.AnyAsync(p => p.Name == request.Name, cancellationToken))
        {
            throw new InvalidOperationException($"Plan with name '{request.Name}' already exists.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var plan = Plan.Create(
            request.Name,
            request.Price,
            request.Currency,
            request.Period,
            request.Type, 
            utcNow
        );

        _monetizationContext.Add(plan);
        await _monetizationContext.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Webhooks.Commands.FulfillSubscription;

public sealed class FulfillSubscriptionHandler : IRequestHandler<FulfillSubscriptionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IPermissionService _permissionService; 

    public FulfillSubscriptionHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IPermissionService permissionService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _permissionService = permissionService;
    }

    public async Task<Unit> Handle(FulfillSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var existingSub = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.Status == SubscriptionStatus.Active, cancellationToken);

        if (existingSub != null) return Unit.Value;

        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);
        if (plan == null) throw new InvalidOperationException("Plan not found.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var endAt = plan.Period == PlanPeriod.Month ? utcNow.AddMonths(1) : utcNow.AddYears(1);

        var subscription = Subscription.Create(
            request.UserId, 
            request.PlanId, 
            utcNow, 
            endAt, 
            SubscriptionStatus.Active, 
            utcNow,
            request.StripeSubscriptionId 
        );

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        await _permissionService.InvalidatePermissionsAsync(request.UserId, cancellationToken);

        return Unit.Value;
    }
}
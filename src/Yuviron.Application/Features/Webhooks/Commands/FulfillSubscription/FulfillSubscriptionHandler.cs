using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Webhooks.Commands.FulfillSubscription;

public sealed class FulfillSubscriptionHandler : IRequestHandler<FulfillSubscriptionCommand, Unit>
{
    private readonly IMonetizationContext _monetizationContext;
    private readonly TimeProvider _timeProvider;
    private readonly IPermissionService _permissionService; 
    private readonly IEventBus _eventBus;

    public FulfillSubscriptionHandler(
        IMonetizationContext monetizationContext, 
        TimeProvider timeProvider,
        IPermissionService permissionService,
        IEventBus eventBus)
    {
        _monetizationContext = monetizationContext;
        _timeProvider = timeProvider;
        _permissionService = permissionService;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(FulfillSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var existingSub = await _monetizationContext.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.Status == SubscriptionStatus.Active, cancellationToken);

        if (existingSub != null) return Unit.Value;

        var plan = await _monetizationContext.Plans.FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);
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

        _monetizationContext.Add(subscription);
        await _monetizationContext.SaveChangesAsync(cancellationToken);

        await _permissionService.InvalidatePermissionsAsync(request.UserId, cancellationToken);

        await _eventBus.PublishAsync(
            new SubscriptionActivatedEvent(request.UserId, plan.Id, plan.Name, endAt),
            cancellationToken);

        return Unit.Value;
    }
}

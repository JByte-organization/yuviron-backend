using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Payment;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.StudioArtist.Payments.Commands.CancelSubscription;

public sealed class CancelSubscriptionHandler : IRequestHandler<CancelSubscriptionCommand, Unit>
{
    private readonly IMonetizationContext _monetizationContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public CancelSubscriptionHandler(
        IMonetizationContext monetizationContext,
        ICurrentUserService currentUser,
        IPaymentService paymentService,
        TimeProvider timeProvider,
        IEventBus eventBus)
    {
        _monetizationContext = monetizationContext;
        _currentUser = currentUser;
        _paymentService = paymentService;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        
        var sub = await _monetizationContext.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.IsAutoRenewing, cancellationToken);

        if (sub == null || string.IsNullOrEmpty(sub.StripeSubscriptionId))
        {
            throw new InvalidOperationException("No active auto-renewing subscription found.");
        }

        await _paymentService.CancelSubscriptionAsync(sub.StripeSubscriptionId, cancellationToken);

        sub.CancelRenewal(_timeProvider.GetUtcNow().UtcDateTime);
        await _monetizationContext.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new SubscriptionCanceledEvent(userId, sub.Plan.Name), cancellationToken);

        return Unit.Value;
    }
}

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

namespace Yuviron.Application.Features.StudioArtist.Payments.Commands.CancelArtistSubscription;

public sealed class CancelArtistSubscriptionHandler : IRequestHandler<CancelArtistSubscriptionCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly IMonetizationContext _monetizationContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus; 

    public CancelArtistSubscriptionHandler(
        ICatalogContext catalogContext, IMonetizationContext monetizationContext, 
        ICurrentUserService currentUser, 
        IPaymentService paymentService,
        TimeProvider timeProvider,
        IEventBus eventBus) 
    {
        _catalogContext = catalogContext;
        _monetizationContext = monetizationContext;
        _currentUser = currentUser;
        _paymentService = paymentService;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(CancelArtistSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        
        var ownsArtist = await _catalogContext.Artists.AnyAsync(a => a.Id == request.ArtistId && a.TeamMembers.Any(tm => tm.UserId == userId), cancellationToken);
        if (!ownsArtist) throw new InvalidOperationException("You do not own this artist profile.");

        var sub = await _monetizationContext.ArtistSubscriptions
            .FirstOrDefaultAsync(s => s.ArtistId == request.ArtistId && s.Status == SubscriptionStatus.Active && s.IsAutoRenewing, cancellationToken);

        if (sub == null || string.IsNullOrEmpty(sub.StripeSubscriptionId))
            throw new InvalidOperationException("No active auto-renewing subscription found for this artist.");

        await _paymentService.CancelSubscriptionAsync(sub.StripeSubscriptionId, cancellationToken);

        sub.CancelRenewal(_timeProvider.GetUtcNow().UtcDateTime);
        await _catalogContext.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new ArtistSubscriptionCanceledEvent(request.ArtistId), cancellationToken);

        return Unit.Value;
    }
}
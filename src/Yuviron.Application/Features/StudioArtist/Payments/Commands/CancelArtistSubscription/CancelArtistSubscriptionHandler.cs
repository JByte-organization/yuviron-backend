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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus; 

    public CancelArtistSubscriptionHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser, 
        IPaymentService paymentService,
        TimeProvider timeProvider,
        IEventBus eventBus) 
    {
        _context = context;
        _currentUser = currentUser;
        _paymentService = paymentService;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(CancelArtistSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        
        var ownsArtist = await _context.Artists.AnyAsync(a => a.Id == request.ArtistId && a.TeamMembers.Any(tm => tm.UserId == userId), cancellationToken);
        if (!ownsArtist) throw new InvalidOperationException("You do not own this artist profile.");

        var sub = await _context.ArtistSubscriptions
            .FirstOrDefaultAsync(s => s.ArtistId == request.ArtistId && s.Status == SubscriptionStatus.Active && s.IsAutoRenewing, cancellationToken);

        if (sub == null || string.IsNullOrEmpty(sub.StripeSubscriptionId))
            throw new InvalidOperationException("No active auto-renewing subscription found for this artist.");

        await _paymentService.CancelSubscriptionAsync(sub.StripeSubscriptionId, cancellationToken);

        sub.CancelRenewal(_timeProvider.GetUtcNow().UtcDateTime);
        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new ArtistSubscriptionCanceledEvent(request.ArtistId), cancellationToken);

        return Unit.Value;
    }
}
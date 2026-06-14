using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums.Monetization;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Finance.Commands.RejectPayout;

public sealed class RejectPayoutHandler : IRequestHandler<RejectPayoutCommand, Unit>
{
    private readonly IMonetizationContext _monetizationContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IEventBus _eventBus;

    public RejectPayoutHandler(IMonetizationContext monetizationContext, TimeProvider timeProvider, IEventBus eventBus, ICurrentUserService currentUser)
    {
        _monetizationContext = monetizationContext; _timeProvider = timeProvider; _eventBus = eventBus; _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RejectPayoutCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var payoutRequest = await _monetizationContext.PayoutRequests
                                .FirstOrDefaultAsync(pr => pr.Id == request.PayoutRequestId, cancellationToken)
                            ?? throw new NotFoundException(nameof(PayoutRequest), request.PayoutRequestId);

        var wallet = await _monetizationContext.ArtistWallets
                         .FirstOrDefaultAsync(w => w.ArtistId == payoutRequest.ArtistId, cancellationToken)
                     ?? throw new InvalidOperationException("Wallet not found.");

        payoutRequest.Reject(adminId, request.Reason, utcNow);
        wallet.ReleaseHeldFunds(payoutRequest.RequestedAmount, utcNow);

        var walletTx = WalletTransaction.Create(
            wallet.Id, payoutRequest.RequestedAmount, WalletTransactionType.PayoutReleased, 
            $"Payout rejected: {request.Reason}", payoutRequest.Id, utcNow);

        _monetizationContext.Add(walletTx);

        await _monetizationContext.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new PayoutRejectedEvent(payoutRequest.ArtistId, payoutRequest.RequestedAmount, request.Reason), cancellationToken);
        return Unit.Value;
    }
}
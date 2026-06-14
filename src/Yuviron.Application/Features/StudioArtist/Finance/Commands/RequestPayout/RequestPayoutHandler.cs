using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging; 
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Enums.Monetization;
using Yuviron.Domain.Events; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Finance.Commands.RequestPayout;

public sealed class RequestPayoutHandler : IRequestHandler<RequestPayoutCommand, Guid>
{
    private readonly ICatalogContext _catalogContext;
    private readonly IMonetizationContext _monetizationContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IEventBus _eventBus;

    public RequestPayoutHandler(
        ICatalogContext catalogContext, IMonetizationContext monetizationContext, 
        TimeProvider timeProvider, 
        ICurrentUserService currentUser,
        IEventBus eventBus) 
    {
        _catalogContext = catalogContext;
        _monetizationContext = monetizationContext; _timeProvider = timeProvider; _currentUser = currentUser; _eventBus = eventBus;
    }

    public async Task<Guid> Handle(RequestPayoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var isOwner = await _catalogContext.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == userId && tm.Role == ArtistTeamRole.Owner, cancellationToken);
        
        if (!isOwner) throw new ForbiddenException("Only the Owner of the artist profile can request a payout.");

        var wallet = await _monetizationContext.ArtistWallets
            .FirstOrDefaultAsync(w => w.ArtistId == request.ArtistId, cancellationToken)
            ?? throw new InvalidOperationException("Wallet not found. No funds available.");

        var settings = await _monetizationContext.ArtistPayoutSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ArtistId == request.ArtistId, cancellationToken);

        if (settings == null || string.IsNullOrWhiteSpace(settings.AccountDetails))
            throw new InvalidOperationException("Please configure your payout method (PayPal/Stripe) before requesting a payout.");

        if (request.Amount < settings.MinWithdrawAmount)
            throw new InvalidOperationException($"Minimum payout amount is {settings.MinWithdrawAmount}");
            
        if (request.Amount > settings.MaxWithdrawAmount)
            throw new InvalidOperationException($"Maximum payout amount is {settings.MaxWithdrawAmount}");

        wallet.HoldFunds(request.Amount, utcNow);

        var payoutRequest = PayoutRequest.Create(request.ArtistId, request.Amount, utcNow);
        _monetizationContext.Add(payoutRequest);

        var walletTx = WalletTransaction.Create(
            wallet.Id, 
            -request.Amount, 
            WalletTransactionType.PayoutReserved, 
            $"Payout request #{payoutRequest.Id.ToString()[..8]}", 
            payoutRequest.Id, 
            utcNow);
        
        _monetizationContext.Add(walletTx);

        try
        {
            await _catalogContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("The wallet balance was modified by another transaction. Please try again.");
        }

        await _eventBus.PublishAsync(new PayoutRequestedEvent(request.ArtistId, request.Amount), cancellationToken);

        return payoutRequest.Id;
    }
}
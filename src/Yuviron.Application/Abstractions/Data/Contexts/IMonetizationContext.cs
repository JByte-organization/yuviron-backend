using Yuviron.Application.Abstractions.Data;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IMonetizationContext : IDataContext
{
    IQueryable<Plan> Plans { get; }
    IQueryable<Subscription> Subscriptions { get; }
    IQueryable<Ad> Ads { get; }
    IQueryable<AdImpression> AdImpressions { get; }
    IQueryable<ArtistPayoutSettings> ArtistPayoutSettings { get; }
    IQueryable<RoyaltyAccrualDaily> RoyaltyAccrualsDaily { get; }
    IQueryable<PayoutRequest> PayoutRequests { get; }
    IQueryable<PayoutTransaction> PayoutTransactions { get; }
    IQueryable<ArtistSubscription> ArtistSubscriptions { get; }
    IQueryable<ArtistWallet> ArtistWallets { get; }
    IQueryable<WalletTransaction> WalletTransactions { get; }
}
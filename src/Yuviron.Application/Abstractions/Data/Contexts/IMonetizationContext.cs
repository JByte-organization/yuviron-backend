using Yuviron.Application.Abstractions.Data;
namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IMonetizationContext : IDataContext
{
    System.Linq.IQueryable<Yuviron.Domain.Entities.Plan> Plans { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Subscription> Subscriptions { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Ad> Ads { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.AdImpression> AdImpressions { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.ArtistPayoutSettings> ArtistPayoutSettings { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.RoyaltyAccrualDaily> RoyaltyAccrualsDaily { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.PayoutRequest> PayoutRequests { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.PayoutTransaction> PayoutTransactions { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.ArtistSubscription> ArtistSubscriptions { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.ArtistWallet> ArtistWallets { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.WalletTransaction> WalletTransactions { get; }
}
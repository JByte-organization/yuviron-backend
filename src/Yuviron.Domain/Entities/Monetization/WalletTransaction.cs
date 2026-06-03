using Yuviron.Domain.Common;
using Yuviron.Domain.Enums.Monetization;

namespace Yuviron.Domain.Entities;

public class WalletTransaction : Entity
{
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; } 
    public WalletTransactionType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    
    public Guid? ReferenceId { get; private set; } 

    public DateTime CreatedAt { get; private set; }

    public virtual ArtistWallet Wallet { get; private set; } = null!;

    private WalletTransaction() { }

    public static WalletTransaction Create(Guid walletId, decimal amount, WalletTransactionType type, string description, Guid? referenceId, DateTime utcNow)
    {
        return new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            Amount = amount,
            Type = type,
            Description = description,
            ReferenceId = referenceId,
            CreatedAt = utcNow
        };
    }
}
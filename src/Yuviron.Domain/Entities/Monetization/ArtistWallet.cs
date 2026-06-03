using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class ArtistWallet : Entity
{
    public Guid ArtistId { get; private set; }
    
    // Скільки грошей доступно для виводу
    public decimal AvailableBalance { get; private set; } 
    
    // Скільки грошей "заморожено" (запит на вивід в обробці)
    public decimal HeldBalance { get; private set; }
    
    // Скільки всього грошей артист заробив за весь час (Life-Time Value)
    public decimal TotalEarned { get; private set; } 

    public DateTime UpdatedAt { get; private set; }
    
    // concurrency token (RowVersion) для захисту від гонки транзакцій при виводі коштів
    public byte[] RowVersion { get; private set; } = null!;

    public virtual Artist Artist { get; private set; } = null!;

    private ArtistWallet() { }

    public static ArtistWallet Create(Guid artistId, DateTime utcNow)
    {
        return new ArtistWallet
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            AvailableBalance = 0,
            HeldBalance = 0,
            TotalEarned = 0,
            UpdatedAt = utcNow
        };
    }

    // Зарахування щоденних роялті
    public void CreditRoyalties(decimal amount, DateTime utcNow)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive");
        AvailableBalance += amount;
        TotalEarned += amount;
        UpdatedAt = utcNow;
    }

    // Заморозка коштів при створенні PayoutRequest
    public void HoldFunds(decimal amount, DateTime utcNow)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive");
        if (AvailableBalance < amount) throw new InvalidOperationException("Insufficient funds");

        AvailableBalance -= amount;
        HeldBalance += amount;
        UpdatedAt = utcNow;
    }

    // Адмін скасував заявку на виплату (повертаємо на баланс)
    public void ReleaseHeldFunds(decimal amount, DateTime utcNow)
    {
        if (HeldBalance < amount) throw new InvalidOperationException("Invalid held amount");

        HeldBalance -= amount;
        AvailableBalance += amount;
        UpdatedAt = utcNow;
    }

    // Адмін підтвердив виплату (списуємо заморожені)
    public void ConfirmPayout(decimal amount, DateTime utcNow)
    {
        if (HeldBalance < amount) throw new InvalidOperationException("Invalid held amount");

        HeldBalance -= amount;
        UpdatedAt = utcNow;
    }
}
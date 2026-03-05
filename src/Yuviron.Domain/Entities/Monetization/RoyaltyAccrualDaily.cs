using System;

namespace Yuviron.Domain.Entities;

public class RoyaltyAccrualDaily
{
    public Guid ArtistId { get; private set; }
    public DateOnly Date { get; private set; } 

    public int StreamsCount { get; private set; }
    public decimal GrossAmount { get; private set; } 
    public decimal PlatformFeeAmount { get; private set; }
    public decimal NetAmount { get; private set; } // Чистыми

    public virtual Artist Artist { get; private set; } = null!;

    private RoyaltyAccrualDaily() { }

    public static RoyaltyAccrualDaily Create(Guid artistId, DateOnly date, int streamsCount, decimal grossAmount, decimal platformFeeAmount)
    {
        if (streamsCount < 0) throw new ArgumentException("Streams cannot be negative");
        if (grossAmount < 0 || platformFeeAmount < 0) throw new ArgumentException("Amounts cannot be negative");

        return new RoyaltyAccrualDaily
        {
            ArtistId = artistId,
            Date = date,
            StreamsCount = streamsCount,
            GrossAmount = grossAmount,
            PlatformFeeAmount = platformFeeAmount,
            // Магия домена: вычисляем чистую прибыль сами, чтобы не было расхождений в БД!
            NetAmount = grossAmount - platformFeeAmount 
        };
    }
}
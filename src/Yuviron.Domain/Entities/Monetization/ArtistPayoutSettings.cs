using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class ArtistPayoutSettings
{
    public Guid ArtistId { get; private set; }

    public decimal MinWithdrawAmount { get; private set; }
    public decimal MaxWithdrawAmount { get; private set; }
    public decimal? CustomRatePerStream { get; private set; } 
    public int PlatformPercent { get; private set; } 

    public virtual Artist Artist { get; private set; } = null!;

    private ArtistPayoutSettings() { }

    public static ArtistPayoutSettings Create(Guid artistId, decimal minWithdraw, decimal maxWithdraw, int platformPercent, decimal? customRate = null)
    {
        if (platformPercent < 0 || platformPercent > 100) throw new ArgumentException("Percent must be between 0 and 100.");
        if (minWithdraw < 0 || maxWithdraw < minWithdraw) throw new ArgumentException("Invalid withdrawal limits.");

        return new ArtistPayoutSettings
        {
            ArtistId = artistId,
            MinWithdrawAmount = minWithdraw,
            MaxWithdrawAmount = maxWithdraw,
            PlatformPercent = platformPercent,
            CustomRatePerStream = customRate
        };
    }
}
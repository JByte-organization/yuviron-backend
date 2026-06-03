using Yuviron.Domain.Common;
using Yuviron.Domain.Enums.Monetization;
using System;

namespace Yuviron.Domain.Entities;

public class ArtistPayoutSettings
{
    public Guid ArtistId { get; private set; }
    public decimal MinWithdrawAmount { get; private set; }
    public decimal MaxWithdrawAmount { get; private set; }
    public decimal? CustomRatePerStream { get; private set; } 
    public int PlatformPercent { get; private set; } 
    
    public PayoutMethod Method { get; private set; }
    public string AccountDetails { get; private set; } = string.Empty; 
    public DateTime UpdatedAt { get; private set; }

    public virtual Artist Artist { get; private set; } = null!;

    private ArtistPayoutSettings() { }

    public static ArtistPayoutSettings Create(
        Guid artistId, decimal minWithdraw, decimal maxWithdraw, 
        int platformPercent, PayoutMethod method, string accountDetails, 
        decimal? customRate = null, DateTime? utcNow = null)
    {
        if (platformPercent < 0 || platformPercent > 100) throw new ArgumentException("Percent must be between 0 and 100.");
        if (minWithdraw < 0 || maxWithdraw < minWithdraw) throw new ArgumentException("Invalid withdrawal limits.");
        if (string.IsNullOrWhiteSpace(accountDetails)) throw new ArgumentException("Account details are required.");

        return new ArtistPayoutSettings
        {
            ArtistId = artistId,
            MinWithdrawAmount = minWithdraw,
            MaxWithdrawAmount = maxWithdraw,
            PlatformPercent = platformPercent,
            CustomRatePerStream = customRate,
            Method = method,
            AccountDetails = accountDetails.Trim(),
            UpdatedAt = utcNow ?? DateTime.UtcNow
        };
    }
    
    public void UpdatePayoutMethod(PayoutMethod method, string accountDetails, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(accountDetails)) throw new ArgumentException("Account details are required.");
        
        Method = method;
        AccountDetails = accountDetails.Trim();
        UpdatedAt = utcNow;
    }
}
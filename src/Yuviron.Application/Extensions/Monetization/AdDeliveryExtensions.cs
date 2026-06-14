using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Extensions;


public record AdDeliveryResult(Guid AdId, string AudioUrl, string ImageUrl,  string AdvertiserName, string Title, string? ClickUrl);

public static class AdDeliveryExtensions
{
    public static async Task<AdDeliveryResult?> GetAdIfCooldownPassedAsync(
        this IMonetizationContext monetizationContext,
        Guid userId,
        DateTime utcNow,
        int cooldownMinutes,
        CancellationToken ct)
    {
        var lastImpression = await monetizationContext.AdImpressions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ShownAt)
            .FirstOrDefaultAsync(ct);

        if (lastImpression == null || (utcNow - lastImpression.ShownAt).TotalMinutes >= cooldownMinutes)
        {
            var randomAd = await monetizationContext.Ads
                .AsNoTracking()
                .Where(a => a.IsActive && !a.IsDeleted)
                .OrderBy(a => Guid.NewGuid()) 
                .FirstOrDefaultAsync(ct);

            if (randomAd != null)
            {
                return new AdDeliveryResult(
                    randomAd.Id, 
                    Path.GetFileName(randomAd.AudioUrl),
                    Path.GetFileName(randomAd.ImageUrl),
                    randomAd.AdvertiserName, 
                    randomAd.Title, 
                    randomAd.ClickUrl);
            }
        }

        return null;
    }
}

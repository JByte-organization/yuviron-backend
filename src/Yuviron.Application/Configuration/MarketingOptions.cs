namespace Yuviron.Application.Configuration;

public class MarketingOptions
{
    public const string SectionName = "Marketing";

    public decimal BannerPricePerDay { get; set; } = 10.00m;
    public string BannerCurrency { get; set; } = "usd";
    public int BannerDurationDays { get; set; } = 7;
}

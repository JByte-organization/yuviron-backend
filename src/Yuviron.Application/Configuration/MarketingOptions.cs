namespace Yuviron.Application.Configuration;

public class MarketingOptions
{
    public const string SectionName = "Marketing";

    public decimal BannerPrice { get; set; } = 50.00m;
    public string BannerCurrency { get; set; } = "usd";
}
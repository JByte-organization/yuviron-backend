namespace Yuviron.Application.Features.Admin.Ads.Queries.GetAds;

public record AdSummaryDto(
    Guid Id,
    string AdvertiserName,
    string Title,
    bool IsActive,
    int ImpressionsCount,
    int ClicksCount,
    DateTime CreatedAt
);